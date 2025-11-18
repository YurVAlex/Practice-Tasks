tailwind.config = {
    theme: {
        extend: {
            colors: {
                'primary-green': '#065f46',
                'secondary-green': '#10b981',
                'overdue-red': '#dc2626',
            }
        }
    }
}

const REMOTE_TASKS_ENDPOINT = 'http://localhost:5146/projectTasks'; // optional endpoint to GET tasks
const REMOTE_UPDATE_ENDPOINT = 'http://localhost:5146/projectUpdate'; // used by sendProjectUpdate();
const REMOTE_PROJECTS_ENDPOINT = '/api/projects'; // list projects in session
const REMOTE_GETPROJECT_ENDPOINT = '/getProject'; 
const REMOTE_POSTPROJECT_ENDPOINT = '/newProject';
const REMOTE_OPENPROJECT_ENDPOINT = '/openProject';

// --- Storage keys (version these if you change the shape later) ---
const STORAGE_KEY_TASKS = 'timeline_tasks_v1';
const STORAGE_KEY_PROJECT = 'timeline_project_v1';

// Helper to generate a new GUID (simple version for client-side)
function generateGuid() {
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {
        const r = Math.random() * 16 | 0;
        const v = c === 'x' ? r : (r & 0x3 | 0x8);
        return v.toString(16);
    });
}

// Default project structure matching the server's Project model
// Use a function to avoid eager GUID generation
function getDefaultProject() {
    return {
        id: generateGuid(),
        tasks: [],
        project: {  // ProjectInfo nested object
            name: "Initial Project Timeline",
            startDate: '2025-10-05',
            endDate: '2025-12-16',
            description: "This is the default, initial project description."
        },
        lastUpdatedTask: null,
        clientTimestamp: new Date().toISOString()
    };
}

// Note: tasks include optional `progress` (number 0-100) and `completed` (boolean).
const defaultTasks = [
    {
        id: 1,
        name: "Phase 1: Concept & Design",
        startDate: "2025-10-18",
        endDate: "2025-11-05",
        description: "Drafting the initial concept and wireframes.",
        completed: true,
        progress: null
    },
    {
        id: 2,
        name: "Phase 2: Development (Incomplete)",
        startDate: "2025-10-07",
        endDate: "2025-10-20",
        description: "Core coding and API integration.",
        completed: false,
        progress: 0
    }
];

// In-memory references (will be initialized from localStorage on load)
let tasks = [];
let currentProject = {};
let pendingDeletionTaskId = null; // for delete-confirm modal
let _deleteModalKeyHandler = null;
let _deleteModalOverlayHandler = null;
let contextMenuTaskId = null; // robust context-menu selection
let projectModalMode = 'current';
let cachedProjectsList = null; // array of {id, name, startDate, endDate}

// ... after 'let cachedProjectsList = null;'

// --- Task Resizing State ---
let isResizing = false;
let resizeTaskId = null;
let resizeHandleType = null; // 'start' or 'end'
let dragStartX = 0;
let originalTaskDate = null;
let daysPerPixel = 0;


/**
 * ----------------------------------------------------------------
 * Task Resizing Handlers (Drag and Drop)
 * ----------------------------------------------------------------
 */

/**
 * Handles mousedown on a resize handle.
 * Initiates the resize drag operation.
 */
function onHandleMouseDown(e, taskId, handleType) {
    // CRITICAL: Stop propagation to prevent task bar's 'onclick' (open modal)
    e.preventDefault();
    e.stopPropagation();

    const task = tasks.find(t => Number(t.id) === Number(taskId));
    if (!task) return;

    // Set global state
    isResizing = true;

    // FIX: Force tooltip to hide immediately when drag starts
    hideTooltip();

    resizeTaskId = taskId;
    resizeHandleType = handleType;
    dragStartX = e.clientX;

    // Store the original date being modified
    originalTaskDate = new Date(task[handleType === 'start' ? 'startDate' : 'endDate'] + 'T00:00:00');

    // Calculate the pixels-to-days conversion factor
    const timelineContainer = document.getElementById('timeline-container');
    const timelineContainerRect = timelineContainer.getBoundingClientRect();
    const timelinePixelWidth = timelineContainerRect.width;

    if (timelineTotalDays <= 0 || timelinePixelWidth <= 0) {
        console.error("Cannot calculate timeline geometry for resize.");
        isResizing = false;
        return;
    }

    // How many days each pixel of drag represents
    daysPerPixel = timelineTotalDays / timelinePixelWidth;

    // Add global listeners to track mouse movement everywhere
    document.addEventListener('mousemove', onHandleMouseMove);
    document.addEventListener('mouseup', onHandleMouseUp);

    // Add body class to set cursor and disable text selection
    document.body.classList.add('is-resizing');
}

/**
 * Handles mousemove during a resize operation.
 * Calculates the new date and shows a tooltip.
 */
function onHandleMouseMove(e) {
    if (!isResizing) return;

    e.preventDefault();

    // Calculate pixel and day deltas
    const dx = e.clientX - dragStartX;
    const dayDelta = Math.round(dx * daysPerPixel);

    // Calculate new date
    const newDate = new Date(originalTaskDate.getTime());
    newDate.setDate(originalTaskDate.getDate() + dayDelta);

    // Find the task to validate against the *other* handle
    const task = tasks.find(t => Number(t.id) === Number(resizeTaskId));
    if (!task) return;

    // --- Live Validation ---
    // Don't let the user drag past the other handle
    if (resizeHandleType === 'start') {
        const endDate = new Date(task.endDate + 'T00:00:00');
        if (newDate >= endDate) {
            // Stop drag from going further
            // Optional: You can show a warning tooltip here if you really want, 
            // but usually stopping the movement is enough feedback.
            return;
        }
    } else { // 'end'
        const startDate = new Date(task.startDate + 'T00:00:00');
        if (newDate <= startDate) {
            return;
        }
    }

    // --- Live DOM Update (Bar Position) ---
    const taskBar = document.getElementById('task-bars-container').querySelector(`[data-task-id="${resizeTaskId}"]`);
    if (taskBar) {
        let visualStartDateStr, visualEndDateStr;

        if (resizeHandleType === 'start') {
            visualStartDateStr = dateToISOString(newDate);
            visualEndDateStr = task.endDate;
        } else { // 'end'
            visualStartDateStr = task.startDate;
            visualEndDateStr = dateToISOString(newDate);
        }

        // Re-calculate bar geometry based on the new virtual dates
        const visualTaskStart = new Date(visualStartDateStr + 'T00:00:00');
        const visualTaskEnd = new Date(visualEndDateStr + 'T00:00:00');

        const startDayOffset = getDayDifference(timelineStartDate, visualTaskStart);
        const endDayOffset = getDayDifference(timelineStartDate, visualTaskEnd);

        const actualStartDay = Math.max(0, startDayOffset);
        const actualEndDay = Math.min(timelineTotalDays, endDayOffset);
        const visibleTaskDuration = actualEndDay - actualStartDay;

        const leftPercent = (actualStartDay / timelineTotalDays) * 100;
        const widthPercent = (visibleTaskDuration / timelineTotalDays) * 100;

        // Apply new styles to the DOM element
        taskBar.style.left = `${leftPercent.toFixed(2)}%`;
        taskBar.style.width = `${widthPercent.toFixed(2)}%`;

        // --- NEW: Live Date Label Update ---

        // 1. Format the new date
        const newMonthStr = newDate.toLocaleString('en-US', { month: 'short' });
        const newDayStr = newDate.getDate();

        // 2. Find the correct side element (start or end)
        let dateDisplayContainer;
        if (resizeHandleType === 'start') {
            dateDisplayContainer = taskBar.querySelector('.task-start-display');
        } else {
            dateDisplayContainer = taskBar.querySelector('.task-end-display');
        }

        // 3. Update the text inside
        if (dateDisplayContainer) {
            const monthSpan = dateDisplayContainer.querySelector('.month-span');
            const daySpan = dateDisplayContainer.querySelector('.day-span');
            if (monthSpan) monthSpan.textContent = newMonthStr;
            if (daySpan) daySpan.textContent = newDayStr;
        }
    }

    // Ensure tooltip is hidden during resize so it doesn't block the view
    hideTooltip();
}

/**
 * Handles mouseup, ending the resize operation.
 * Validates, saves the new date, and re-renders.
 */
function onHandleMouseUp(e) {
    if (!isResizing) return;

    e.preventDefault();

    // --- 1. Clean up global state and listeners ---
    isResizing = false;
    document.removeEventListener('mousemove', onHandleMouseMove);
    document.removeEventListener('mouseup', onHandleMouseUp);
    document.body.classList.remove('is-resizing');
    hideTooltip();

    // --- 2. Find the task to update ---
    const task = tasks.find(t => Number(t.id) === Number(resizeTaskId));
    if (!task) {
        resizeTaskId = null;
        renderTasks(); // Task not found? Re-render to be safe.
        return;
    }

    // --- 3. Calculate final new date ---
    const dx = e.clientX - dragStartX;
    const dayDelta = Math.round(dx * daysPerPixel);

    // If no significant change, just snap back
    if (dayDelta === 0) {
        resizeTaskId = null;
        renderTasks(); // Snap back
        return;
    }

    const newDate = new Date(originalTaskDate.getTime());
    newDate.setDate(originalTaskDate.getDate() + dayDelta);
    const newDateStr = dateToISOString(newDate);

    // --- 4. Final Validation ---
    let canSave = false; // Default to false

    if (resizeHandleType === 'start') {
        const endDate = new Date(task.endDate + 'T00:00:00');
        if (newDate >= endDate) {
            showToast('Start date must be before end date.', 'error');
        } else {
            task.startDate = newDateStr;
            canSave = true;
        }
    } else { // 'end'
        const startDate = new Date(task.startDate + 'T00:00:00');
        if (newDate <= startDate) {
            showToast('End date must be after start date.', 'error');
        } else {
            task.endDate = newDateStr;
            canSave = true;
        }
    }

    // --- 5. Save and Re-render (on success) OR Revert (on fail) ---
    if (canSave) {
        saveTasksToStorage();
        showToast(`Task "${task.name}" dates updated.`, 'success');

        // This will re-render tasks and expand timeline if needed
        checkAndUpdateTimeline();
        sendProjectUpdate();
    } else {
        // Validation failed. Snap back to original state from `tasks` array.
        renderTasks();
    }

    // Clear task ID
    resizeTaskId = null;
}

// Tooltip element reference
let tooltipEl = null;

// --- Helper: normalize task status ---
// Ensures:
// - completed is boolean
// - progress is either null or a number 0..100
// - if progress >= 100 => mark completed = true and progress = null
function normalizeTask(task) {
    if (!task || typeof task !== 'object') return;

    // Ensure completed exists as boolean
    task.completed = !!task.completed;

    // Normalize progress presence
    if (!('progress' in task) || task.progress === undefined || task.progress === null) {
        // If completed -> no progress; otherwise default to 0
        task.progress = task.completed ? null : 0;
    } else {
        // coerce to number when possible
        const p = Number(task.progress);
        if (!isNaN(p)) {
            // clamp between 0 and 100
            const clamped = Math.max(0, Math.min(100, Math.round(p)));
            task.progress = clamped;
        } else {
            task.progress = task.completed ? null : 0;
        }
    }

    // If progress indicates fully done, mark completed and clear progress
    if (typeof task.progress === 'number' && task.progress >= 100) {
        task.completed = true;
        task.progress = null;
    }

    // If completed is true, clear progress to avoid conflicting states
    if (task.completed) {
        task.progress = null;
    }
}

// --- Persistence helpers ---
function loadTasksFromStorage() {
    const raw = localStorage.getItem(STORAGE_KEY_TASKS);
    if (raw) {
        try {
            const parsed = JSON.parse(raw);
            if (Array.isArray(parsed)) {
                // Normalize tasks (backwards compatibility)
                tasks = parsed.map(t => {
                    // Shallow copy to avoid mutating stored object unexpectedly
                    const copy = Object.assign({}, t);
                    normalizeTask(copy);
                    return copy;
                });
                // Ensure saved normalized values are persisted
                saveTasksToStorage();
                return;
            }
        } catch (e) {
            console.error('Failed to parse stored tasks, resetting to defaults.', e);
        }
    }
    // fallback to defaults
    tasks = defaultTasks.map(t => {
        const copy = Object.assign({}, t);
        normalizeTask(copy);
        return copy;
    });
    saveTasksToStorage();
}

function saveTasksToStorage() {
    try {
        // Normalize all tasks before saving to keep consistency
        tasks.forEach(t => normalizeTask(t));
        localStorage.setItem(STORAGE_KEY_TASKS, JSON.stringify(tasks));
    } catch (e) {
        console.error('Failed to save tasks to localStorage', e);
    }
}

function loadProjectFromStorage() {
    const raw = localStorage.getItem(STORAGE_KEY_PROJECT);
    if (raw) {
        try {
            currentProject = JSON.parse(raw);
            if (!currentProject) {
                currentProject = getDefaultProject();
            }
            // Ensure id exists (for backward compatibility with old data)
            // CONSIDER TO DELETE THAT
            if (!currentProject.id) {
                currentProject.id = generateGuid();
                console.warn('[loadProjectFromStorage] Project missing id, generated:', currentProject.id);
                saveProjectToStorage();
            }
            return;
        } catch (e) {
            console.error('Failed to parse stored project, resetting to default.', e);
        }
    }
    currentProject = getDefaultProject();
    console.log('[loadProjectFromStorage] Created default project with id:', currentProject.id);
    saveProjectToStorage();
}

function saveProjectToStorage() {
    try {
        localStorage.setItem(STORAGE_KEY_PROJECT, JSON.stringify(currentProject));
    } catch (e) {
        console.error('Failed to save project to localStorage', e);
    }
}

async function sendProjectUpdate() {

    const payload = {
        id: currentProject.id,
        tasks: tasks, 
        project: currentProject.project,
        lastUpdatedTask: currentProject.lastUpdatedTask,
        clientTimestamp: new Date().toISOString()
    };
    
    console.log('[sendProjectUpdate] Sending project with id:', payload.id, 'Name:', payload.project?.name);

    try {
        const response = await fetch(REMOTE_UPDATE_ENDPOINT, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(payload),
            credentials: 'include' // <--- THIS IS CRITICAL FOR SENDING COOKIES
        });
        if (response.status === 401) {
            // Unauthorized
            console.error("Authentication Failed: User is Unauthorized, or session management error.");
            showToast(`Authentication Failed. Remote project are not updated!`, 'info');
            // TODO Redirect to login page ???
        }
        if (response.status === 400) {
            const warnData = await response.json();
            // non-2xx responses are still considered "successful fetch" but report them
            console.warn(`[sendProjectUpdate] projectUpdate returned status ${response.status}`);
            console.warn(`[sendProjectUpdate] ${warnData.error}`);
            showToast(`${warnData.error}`, 'info');
        }
        if (response.status === 200) {
            const okData = await response.json();
            // non-2xx responses are still considered "successful fetch" but report them
            console.log(`[sendProjectUpdate] Successful remote update! ${okData.savedFor} updated, ${okData.receivedTasks} tasks sended.`);
            showToast(`Remote update ${okData.savedFor}: done.`, 'success');
        }
        return response;
    } catch (error) {
        // Network / fetch error — keep local storage authoritative but surface the problem to console
        console.error('Failed to send projectUpdate payload:', error);
        // Optionally you could return a rejected promise or a custom object to indicate failure
        return null;
    }
}

// Optionally fetch remote tasks and merge onto local storage (not enabled by default)
/*async function tryFetchRemoteTasksOnLoad() {
    if (!REMOTE_SYNC_ON_LOAD) return;
    try {
        const response = await fetch(REMOTE_TASKS_ENDPOINT);
        if (!response.ok) return;
        const remoteTasks = await response.json();
        if (Array.isArray(remoteTasks)) {
            // Very simple merge: prefer remote tasks (could be changed to two-way merge)
            tasks = remoteTasks.map(t => {
                const copy = Object.assign({}, t);
                normalizeTask(copy);
                return copy;
            });
            saveTasksToStorage();
        }
    } catch (e) {
        console.warn('Remote tasks fetch failed:', e);
    }
}
*/
// --- Helper date functions ---
const getDayDifference = (date1, date2) => {
    const oneDay = 1000 * 60 * 60 * 24;
    const diffTime = date2.getTime() - date1.getTime();
    return Math.round(diffTime / oneDay);
};

const dateToISOString = (date) => {
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
};

// --- Toast UI helper ---
const showToast = (message, type = 'info', duration = 4000) => {
    const container = document.getElementById('toast-container');
    const toast = document.createElement('div');

    let bgColor = 'bg-gray-800';
    let iconHtml = '';

    if (type === 'success') {
        bgColor = 'bg-primary-green';
        iconHtml = '<svg class="w-5 h-5 mr-2 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"></path></svg>';
    } else if (type === 'error') {
        bgColor = 'bg-overdue-red';
        iconHtml = '<svg class="w-5 h-5 mr-2 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 14l2-2m0 0l2-2m-2 2l-2-2m2 2l2 2m7-2a9 9 0 11-18 0 9 9 0 0118 0z"></path></svg>';
    } else {
        bgColor = 'bg-blue-500';
        iconHtml = '<svg class="w-5 h-5 mr-2 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"></path></svg>';
    }

    toast.className = `flex items-center px-6 py-3 text-white text-sm font-medium rounded-lg shadow-xl mb-3 transition-all duration-300 transform translate-y-0 opacity-100 ${bgColor}`;
    toast.style.pointerEvents = 'auto';
    toast.innerHTML = iconHtml + message;

    container.appendChild(toast);

    setTimeout(() => {
        toast.classList.replace('opacity-100', 'opacity-0');
        toast.classList.replace('translate-y-0', 'translate-y-4');
        setTimeout(() => {
            toast.remove();
        }, 300);
    }, duration);
};

// --- Timeline update logic ---
let timelineStartDate = new Date('2025-10-05T00:00:00');
let timelineEndDate = new Date('2025-12-16T00:00:00');
let timelineTotalDays = 0;

/*
  Centralized function: fitTimelineToTasks(shrinkAllowed)
  - shrinkAllowed = true  => fit project bounds exactly to tasks (used by Scale and deletions)
  - shrinkAllowed = false => expand project bounds only if tasks fall outside (used after add/clone/edit)
  Important fix: always re-render task bars (renderTasks()) after tasks change, even if project bounds don't change.
*/
const fitTimelineToTasks = (shrinkAllowed = true) => {
    if (!Array.isArray(tasks) || tasks.length === 0) {
        // no tasks -> revert to defaults (preserve existing Id if possible)
        const existingId = currentProject.id;
        currentProject = getDefaultProject();
        if (existingId) {
            currentProject.id = existingId;  // Preserve the Id from server
        }
        saveProjectToStorage();
        renderProjectInfo();
        const projectInfo = currentProject.project || {};
        renderTimeline(projectInfo.startDate, projectInfo.endDate);
        showToast('No tasks present — timeline reset to default project bounds.', 'info');
        return;
    }

    let minStart = null;
    let maxEnd = null;

    tasks.forEach(t => {
        const s = new Date(t.startDate + 'T00:00:00');
        const e = new Date(t.endDate + 'T00:00:00');
        if (isNaN(s.getTime()) || isNaN(e.getTime())) return;
        if (minStart === null || s < minStart) minStart = s;
        if (maxEnd === null || e > maxEnd) maxEnd = e;
    });

    if (!minStart || !maxEnd) {
        // fallback (preserve existing Id if possible)
        const existingId = currentProject.id;
        currentProject = getDefaultProject();
        if (existingId) {
            currentProject.id = existingId;  // Preserve the Id from server
        }
        saveProjectToStorage();
        renderProjectInfo();
        const projectInfo = currentProject.project || {};
        renderTimeline(projectInfo.startDate, projectInfo.endDate);
        showToast('Unable to calculate task bounds — timeline reset to defaults.', 'error');
        return;
    }

    const newStartStr = dateToISOString(minStart);
    const newEndStr = dateToISOString(maxEnd);

    const projectInfo = currentProject.project || {};
    const currentStartStr = projectInfo.startDate;
    const currentEndStr = projectInfo.endDate;

    if (shrinkAllowed) {
        // set to exact min/max
        if (newStartStr !== currentStartStr || newEndStr !== currentEndStr) {
            if (!currentProject.project) currentProject.project = {};
            currentProject.project.startDate = newStartStr;
            currentProject.project.endDate = newEndStr;
            currentProject.clientTimestamp = new Date().toISOString();
            saveProjectToStorage();
            renderProjectInfo();
            renderTimeline(newStartStr, newEndStr);
            // renderTimeline calls renderTasks internally
            showToast(`Timeline adjusted to fit tasks (${newStartStr} to ${newEndStr}).`, 'info');
            return;
        } else {
            // No project bounds change; still need to redraw task bars (e.g., after clone/add/delete)
            renderTasks();
            showToast('Timeline already fits all tasks.', 'info');
            return;
        }
    } else {
        // expand-only behavior: only modify start if earlier, end if later
        let changed = false;
        const curStartDate = new Date(currentStartStr + 'T00:00:00');
        const curEndDate = new Date(currentEndStr + 'T00:00:00');

        let updatedStart = currentStartStr;
        let updatedEnd = currentEndStr;

        if (minStart < curStartDate) {
            updatedStart = newStartStr;
            changed = true;
        }
        if (maxEnd > curEndDate) {
            updatedEnd = newEndStr;
            changed = true;
        }

        if (changed) {
            if (!currentProject.project) currentProject.project = {};
            currentProject.project.startDate = updatedStart;
            currentProject.project.endDate = updatedEnd;
            currentProject.clientTimestamp = new Date().toISOString();
            saveProjectToStorage();
            renderProjectInfo();
            renderTimeline(updatedStart, updatedEnd);
            showToast(`Timeline expanded to include task bounds (${updatedStart} to ${updatedEnd}).`, 'success');
            return;
        } else {
            // No bounds change but tasks array may have changed -> re-render task bars so new/edited/cloned tasks show up
            renderTasks();
            showToast('No expansion needed — timeline already includes all tasks.', 'info');
            return;
        }
    }
};

const checkAndUpdateTimeline = () => {
    // Expand-only behavior when tasks are added/edited/cloned
    fitTimelineToTasks(false); // expand only (do not shrink)
};

// --- adjustTimelineAfterDeletion now uses the same common function to fit exactly ---
const adjustTimelineAfterDeletion = () => {
    fitTimelineToTasks(true); // shrink allowed: fit exactly
};

// --- Project modal behavior (saving project persists to storage) ---
const openProjectModal = () => {
    const modal = document.getElementById('project-modal');
    // Access nested ProjectInfo object
    const projectInfo = currentProject.project || {};
    document.getElementById('project-modal-name').value = projectInfo.name || '';
    document.getElementById('project-modal-start-date').value = projectInfo.startDate || '';
    document.getElementById('project-modal-end-date').value = projectInfo.endDate || '';
    document.getElementById('project-modal-description').value = projectInfo.description || '';
    document.getElementById('project-modal-status').textContent = '';
    projectModalMode = 'current';
    setProjectModalTab('current');
    modal.classList.remove('hidden');
};

const closeProjectModal = () => {
    document.getElementById('project-modal').classList.add('hidden');
};

// Create a new project and navigate to it
const createAndLoadNewProject = async (projectName, startDate, endDate, description) => {
    const newProject = {
        id: generateGuid(),
        tasks: [],
        project: {
            name: projectName,
            startDate: startDate,
            endDate: endDate,
            description: description
        },
        lastUpdatedTask: null,
        clientTimestamp: new Date().toISOString()
    };
    
    console.log('[createAndLoadNewProject] Creating new project:', newProject.id);
    console.log('[createAndLoadNewProject] Project name:', newProject.project.name);
    
    try {
        const response = await fetch(REMOTE_POSTPROJECT_ENDPOINT, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(newProject),
            credentials: 'include'
        });
        
        if (response.ok) {
            const okData = await response.json();
            showToast(`Success - added new project. ${okData.success}`, 'success');
            console.log('[createAndLoadNewProject] Success - added new project.', okData.success);
            window.location.href = REMOTE_GETPROJECT_ENDPOINT;
        } else {
            const errorData = await response.json().catch(() => ({ error: 'Unknown error' }));
            showToast(`Failed to create project: ${errorData.error || response.statusText}`, 'error');
            console.error('[createAndLoadNewProject] Server error:', errorData);
        }
    } catch (error) {
        showToast(`Network error: ${error.message}`, 'error');
        console.error('[createAndLoadNewProject] Network error:', error);
    }
};

const updateProjectModalState = (mode) => {
    const dateFields = document.getElementById('project-date-fields');
    const descriptionField = document.getElementById('project-description-field');
    const okButton = document.getElementById('project-modal-ok');
    const modalName = document.getElementById('project-modal-name');
    const openList = document.getElementById('project-open-list');

    if (mode === 'current') {
        dateFields.classList.remove('hidden');
        descriptionField.classList.remove('hidden');
        if (openList) openList.classList.add('hidden');
        okButton.textContent = 'OK';
        modalName.readOnly = false;
        modalName.classList.remove('bg-gray-100');
    } else if (mode === 'new') {
        dateFields.classList.remove('hidden');
        descriptionField.classList.remove('hidden');
        if (openList) openList.classList.add('hidden');
        okButton.textContent = 'Create Project';
        modalName.readOnly = false;
        modalName.value = '';
        modalName.classList.remove('bg-gray-100');
    } else if (mode === 'open') {
        // Show list, hide other fields
        dateFields.classList.add('hidden');
        descriptionField.classList.add('hidden');
        if (openList) openList.classList.remove('hidden');
        okButton.textContent = 'Close';
        modalName.readOnly = true;
        modalName.classList.add('bg-gray-100');
    }
};

function setProjectModalTab(mode) {
    const curBtn = document.getElementById('tab-project-current');
    const newBtn = document.getElementById('tab-project-new');
    const openBtn = document.getElementById('tab-project-open');
    if (!curBtn || !newBtn) {
        projectModalMode = mode;
        updateProjectModalState(mode);
        return;
    }
    const base = 'px-3 py-2 text-sm font-semibold border-b-2 ';
    const active = 'border-primary-green text-primary-green';
    const inactive = 'border-transparent text-gray-500 hover:text-gray-700';
    curBtn.className = base + (mode === 'current' ? active : inactive);
    newBtn.className = base + (mode === 'new' ? active : inactive);
    if (openBtn) openBtn.className = base + (mode === 'open' ? active : inactive);
    curBtn.setAttribute('aria-selected', mode === 'current' ? 'true' : 'false');
    newBtn.setAttribute('aria-selected', mode === 'new' ? 'true' : 'false');
    if (openBtn) openBtn.setAttribute('aria-selected', mode === 'open' ? 'true' : 'false');
    projectModalMode = mode;
    updateProjectModalState(mode);
    if (mode === 'open') {
        ensureProjectsListLoadedAndRender();
    }
}

const handleProjectModalOk = async () => {
    const projectName = document.getElementById('project-modal-name').value.trim();
    const projectMode = projectModalMode;
    const modalStatus = document.getElementById('project-modal-status');
    modalStatus.textContent = '';

    if (projectMode === 'open') {
        closeProjectModal();
        return;
    }

    if (!projectName) {
        modalStatus.textContent = 'Project Name is required.';
        return;
    }

    const startDateStr = document.getElementById('project-modal-start-date').value;
    const endDateStr = document.getElementById('project-modal-end-date').value;
    const projectDescription = document.getElementById('project-modal-description').value.trim();

    if (!startDateStr || !endDateStr) {
        modalStatus.textContent = 'Start Date and End Date are required.';
        return;
    }

    const taskStart = new Date(startDateStr + 'T00:00:00');
    const taskEnd = new Date(endDateStr + 'T00:00:00');

    if (taskStart >= taskEnd) {
        modalStatus.textContent = 'Project End Date must be after the Start Date.';
        return;
    }

    if (projectMode === 'new') {
        // Create new project and navigate to it
        await createAndLoadNewProject(projectName, startDateStr, endDateStr, projectDescription);
        return;
    }

    // Preserve the existing id and update the nested ProjectInfo
    const existingId = currentProject.id;
    if (!existingId) {
        console.error('[handleProjectModalOk] CRITICAL: currentProject.id is missing!');
        console.error('[handleProjectModalOk] currentProject:', currentProject);
    }
    
    currentProject = {
        id: existingId || generateGuid(),  // Should never need to generate new
        tasks: tasks,  // Include current tasks
        project: {  // Update nested ProjectInfo object
            name: projectName,
            startDate: startDateStr,
            endDate: endDateStr,
            description: projectDescription
        },
        lastUpdatedTask: null,
        clientTimestamp: new Date().toISOString()
    };
    
    console.log('[handleProjectModalOk] Updated project with id:', currentProject.id);
    console.log('[handleProjectModalOk] Project name:', currentProject.project.name);

    saveProjectToStorage();
    closeProjectModal();
    renderProjectInfo();
    renderTimeline(startDateStr, endDateStr);
    showToast(`Project boundaries successfully updated.`, 'info');
    sendProjectUpdate();
    
};

async function ensureProjectsListLoadedAndRender(force = false) {
    if (cachedProjectsList === null || force) {
        try {
            const resp = await fetch(REMOTE_PROJECTS_ENDPOINT, { credentials: 'include' });
            if (resp.ok) {
                cachedProjectsList = await resp.json();
            } else {
                cachedProjectsList = [];
            }
        } catch (e) {
            console.error('Failed to load projects list', e);
            cachedProjectsList = [];
        }
    }
    renderProjectsList(cachedProjectsList || []);
}

function renderProjectsList(items) {
    const body = document.getElementById('project-open-list-body');
    if (!body) return;
    body.innerHTML = '';
    if (!Array.isArray(items) || items.length === 0) {
        const empty = document.createElement('div');
        empty.className = 'p-3 text-sm text-gray-600';
        empty.textContent = 'No projects available.';
        body.appendChild(empty);
        return;
    }
    items.forEach(p => {
        const row = document.createElement('button');
        row.type = 'button';
        row.className = 'w-full text-left p-3 hover:bg-gray-50 flex items-center justify-between';
        row.setAttribute('data-id', p.id);
        row.addEventListener('click', () => openExistingProject(p.id, p.name));
        const name = document.createElement('span');
        name.className = 'text-sm font-medium text-gray-800';
        name.textContent = p.name || '(unnamed)';
        const dates = document.createElement('span');
        dates.className = 'text-xs text-gray-500';
        
        if (p.timestamp) {
            dates.textContent = `${p.timestamp.toLocaleString()}`;
        }
        row.appendChild(name);
        row.appendChild(dates);
        body.appendChild(row);
    });
}

async function openExistingProject(projectId, projectName) {
    try {
        const payload = {
            id: projectId,
            tasks: [],
            project: {
                name: projectName,
                startDate: null,
                endDate: null,
                description: ''
            },
            lastUpdatedTask: null,
            clientTimestamp: new Date().toISOString()
        };
        const response = await fetch(REMOTE_OPENPROJECT_ENDPOINT, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload),
            credentials: 'include'
        });
        if (response.ok) {
            const okData = await response.json();
            showToast(`Success - open new project. ${okData.success}`, 'success');
            console.log('[openExistingProject] Success - open new project.', okData.success);
            window.location.href = REMOTE_GETPROJECT_ENDPOINT;
        } else {
            showToast('Failed to open project.', 'error');
        }
    } catch (e) {
        console.error('Open project failed', e);
        showToast('Network error while opening project.', 'error');
    }
}

// --- QUICK SCALE modal helpers (new) ---
// Opens the compact modal used when clicking the progress line.
// Only allows editing start/end dates and OK/Cancel.
const openScaleModal = () => {
    const modal = document.getElementById('scale-modal');
    if (!modal) return;

    // Populate fields from nested ProjectInfo
    const projectInfo = currentProject.project || {};
    document.getElementById('scale-modal-start-date').value = projectInfo.startDate || '';
    document.getElementById('scale-modal-end-date').value = projectInfo.endDate || '';
    document.getElementById('scale-modal-status').textContent = '';

    modal.classList.remove('hidden');
    document.body.classList.add('modal-open');

    // focus start date for accessibility
    setTimeout(() => {
        const startInput = document.getElementById('scale-modal-start-date');
        if (startInput) startInput.focus();
    }, 0);
};

const closeScaleModal = () => {
    const modal = document.getElementById('scale-modal');
    if (!modal) return;
    modal.classList.add('hidden');
    document.body.classList.remove('modal-open');
};

const handleScaleModalOk = () => {
    const status = document.getElementById('scale-modal-status');
    status.textContent = '';

    const startDateStr = document.getElementById('scale-modal-start-date').value;
    const endDateStr = document.getElementById('scale-modal-end-date').value;

    if (!startDateStr || !endDateStr) {
        status.textContent = 'Start Date and End Date are required.';
        return;
    }
    const s = new Date(startDateStr + 'T00:00:00');
    const e = new Date(endDateStr + 'T00:00:00');

    if (isNaN(s.getTime()) || isNaN(e.getTime())) {
        status.textContent = 'Invalid date format.';
        return;
    }
    if (s >= e) {
        status.textContent = 'End date must be after start date.';
        return;
    }

    // Update project bounds in nested ProjectInfo, persist, and re-render timeline
    if (!currentProject.project) currentProject.project = {};
    currentProject.project.startDate = startDateStr;
    currentProject.project.endDate = endDateStr;
    currentProject.clientTimestamp = new Date().toISOString();
    saveProjectToStorage();
    renderProjectInfo();
    renderTimeline(currentProject.project.startDate, currentProject.project.endDate);

    showToast('Timeline updated.', 'success');
    closeScaleModal();
    sendProjectUpdate();
};

// --- Task modal (add/edit) ---
const openTaskModal = (taskId = null) => {
    const modal = document.getElementById('task-modal');
    const saveButton = document.getElementById('save-task-ok');
    const modalTitle = document.querySelector('#task-modal h2');

    // Reset fields
    document.getElementById('task-name').value = '';
    document.getElementById('task-start-date').value = '';
    document.getElementById('task-end-date').value = '';
    document.getElementById('task-description').value = '';
    document.getElementById('task-modal-status').textContent = '';

    // Reset new status radios and progress
    const completedRadio = document.getElementById('task-status-completed');
    const progressRadio = document.getElementById('task-status-progress');
    const progressInput = document.getElementById('task-progress');

    if (completedRadio) completedRadio.checked = false;
    if (progressRadio) progressRadio.checked = false;
    if (progressInput) {
        progressInput.value = 0;
        progressInput.disabled = true;
    }

    // Default to Add mode
    saveButton.removeAttribute('data-task-id');
    saveButton.textContent = 'Add Task';
    modalTitle.textContent = 'Add New Task';

    if (taskId !== null) {
        const taskToEdit = tasks.find(t => Number(t.id) === Number(taskId));
        if (taskToEdit) {
            document.getElementById('task-name').value = taskToEdit.name;
            document.getElementById('task-start-date').value = taskToEdit.startDate;
            document.getElementById('task-end-date').value = taskToEdit.endDate;
            document.getElementById('task-description').value = taskToEdit.description;

            // Normalize before populating (safety)
            normalizeTask(taskToEdit);

            // Populate status controls: if completed -> completed radio, else progress radio with value
            if (taskToEdit.completed) {
                if (completedRadio) completedRadio.checked = true;
                if (progressInput) {
                    progressInput.value = 0;
                    progressInput.disabled = true;
                }
            } else {
                if (progressRadio) progressRadio.checked = true;
                if (progressInput) {
                    progressInput.value = (typeof taskToEdit.progress === 'number') ? Math.min(100, Math.max(0, Math.round(taskToEdit.progress))) : 0;
                    progressInput.disabled = false;
                }
            }

            saveButton.setAttribute('data-task-id', taskId);
            saveButton.textContent = 'Save Changes';
            modalTitle.textContent = 'Edit Task';
        }
    } else {
        // Default new tasks to progress=0 (not completed)
        if (progressRadio) progressRadio.checked = true;
        if (progressInput) {
            progressInput.value = 0;
            progressInput.disabled = false;
        }
    }

    // Wire up the status radios to enable/disable the progress input (re-attach handlers cleanly)
    const updateProgressAvailability = () => {
        const prog = document.getElementById('task-progress');
        const pr = document.getElementById('task-status-progress');
        if (prog) {
            if (pr && pr.checked) {
                prog.disabled = false;
            } else {
                prog.disabled = true;
                prog.value = 0;
            }
        }
    };

    // Remove prior listeners by cloning nodes to avoid duplicate handlers if modal opened multiple times
    const completedNode = document.getElementById('task-status-completed');
    const progressNode = document.getElementById('task-status-progress');

    if (completedNode) {
        const newCompleted = completedNode.cloneNode(true);
        completedNode.parentNode.replaceChild(newCompleted, completedNode);
    }
    if (progressNode) {
        const newProgress = progressNode.cloneNode(true);
        progressNode.parentNode.replaceChild(newProgress, progressNode);
    }

    // Attach listeners to freshly replaced nodes
    const completedNodeFresh = document.getElementById('task-status-completed');
    const progressNodeFresh = document.getElementById('task-status-progress');
    if (completedNodeFresh) completedNodeFresh.addEventListener('change', updateProgressAvailability);
    if (progressNodeFresh) progressNodeFresh.addEventListener('change', updateProgressAvailability);

    modal.classList.remove('hidden');
};

const closeTaskModal = () => {
    document.getElementById('task-modal').classList.add('hidden');
};

const saveTask = () => {
    const taskName = document.getElementById('task-name').value.trim();
    const taskStartDateStr = document.getElementById('task-start-date').value;
    const taskEndDateStr = document.getElementById('task-end-date').value;
    const taskDescription = document.getElementById('task-description').value.trim();
    const modalStatus = document.getElementById('task-modal-status');
    const saveButton = document.getElementById('save-task-ok');
    const editingTaskId = saveButton.getAttribute('data-task-id');

    modalStatus.textContent = '';

    if (!taskName || !taskStartDateStr || !taskEndDateStr) {
        modalStatus.textContent = 'Please fill in the Task Name, Start Date, and End Date.';
        return;
    }

    const taskStart = new Date(taskStartDateStr + 'T00:00:00');
    const taskEnd = new Date(taskEndDateStr + 'T00:00:00');

    if (isNaN(taskStart.getTime()) || isNaN(taskEnd.getTime())) {
        modalStatus.textContent = 'Invalid date format.';
        return;
    }

    if (taskStart >= taskEnd) {
        modalStatus.textContent = 'Task End Date must be after the Start Date.';
        return;
    }

    // Read status radios and progress input
    const status = document.querySelector('input[name="task-status"]:checked');
    let completedVal = false;
    let progressVal = null;

    if (status && status.value === 'completed') {
        completedVal = true;
        progressVal = null;
    } else if (status && status.value === 'progress') {
        completedVal = false;
        const raw = document.getElementById('task-progress').value;
        let parsed = parseInt(raw);
        if (isNaN(parsed)) parsed = 0;
        parsed = Math.max(0, Math.min(100, parsed));
        progressVal = parsed;
    } else {
        // Fallback: not completed, progress 0
        completedVal = false;
        progressVal = 0;
    }

    // Automatic rule: if progressVal === 100 => mark completed and clear progress
    if (typeof progressVal === 'number' && progressVal >= 100) {
        completedVal = true;
        progressVal = null;
    }

    let message = '';

    if (editingTaskId) {
        // Update existing task
        const taskIdNumber = parseInt(editingTaskId);
        let taskToUpdate = tasks.find(t => Number(t.id) === taskIdNumber);
        if (taskToUpdate) {
            taskToUpdate.name = taskName;
            taskToUpdate.startDate = taskStartDateStr;
            taskToUpdate.endDate = taskEndDateStr;
            taskToUpdate.description = taskDescription;
            taskToUpdate.completed = completedVal;
            taskToUpdate.progress = progressVal;
            // normalize to enforce invariants (especially when progress reached 100)
            normalizeTask(taskToUpdate);
            message = `Task "${taskName}" updated successfully.`;
            saveTasksToStorage();
        }
    } else {
        // Add new task
        const newTaskId = Date.now();
        const newTask = {
            id: newTaskId,
            name: taskName,
            startDate: taskStartDateStr,
            endDate: taskEndDateStr,
            description: taskDescription,
            completed: completedVal,
            progress: progressVal
        };
        // normalize right away (handles progress==100 => completed)
        normalizeTask(newTask);
        tasks.push(newTask);
        message = `New task "${taskName}" added successfully.`;
        saveTasksToStorage();
    }

    closeTaskModal();
    showToast(message, 'success');

    // After adding/updating tasks: ensure timeline expands if necessary (expand-only)
    checkAndUpdateTimeline();
    sendProjectUpdate();
};

// --- Context menu logic (Clone persists to storage) ---
const showContextMenu = (e, taskId) => {
    e.preventDefault();
    const menu = document.getElementById('context-menu');
    menu.classList.add('hidden');

    const viewportWidth = window.innerWidth;
    const viewportHeight = window.innerHeight;
    const menuWidth = 160;
    const menuHeight = 280;
    let left = e.clientX;
    let top = e.clientY;

    if (left + menuWidth > viewportWidth) {
        left = viewportWidth - menuWidth - 10;
    }
    if (top + menuHeight > viewportHeight) {
        top = viewportHeight - menuHeight - 10;
    }

    menu.style.left = `${left}px`;
    menu.style.top = `${top}px`;

    // Robust: set both attribute and global variable
    menu.setAttribute('data-task-id', taskId);
    contextMenuTaskId = Number(taskId);

    // Also hide tooltip when context menu opens
    hideTooltip();

    menu.classList.remove('hidden');
};

const openDeleteConfirmModal = (taskId) => {
    const modal = document.getElementById('delete-confirm-modal');
    const messageEl = document.getElementById('delete-confirm-message');
    pendingDeletionTaskId = Number(taskId);

    // Hide context menu if it's visible
    const ctx = document.getElementById('context-menu');
    if (ctx) ctx.classList.add('hidden');

    const task = tasks.find(t => Number(t.id) === Number(taskId));
    if (task) {
        messageEl.textContent = `Are you sure you want to delete "${task.name}"? This action cannot be undone.`;
    } else {
        messageEl.textContent = 'Are you sure you want to delete this task? This action cannot be undone.';
    }

    modal.classList.remove('hidden');

    // Prevent background scrolling while modal is open
    document.body.classList.add('modal-open');

    // Focus confirmation button for accessibility
    setTimeout(() => {
        const okBtn = document.getElementById('delete-confirm-ok');
        if (okBtn) okBtn.focus();
    }, 0);

    // Key handler (Escape to cancel)
    _deleteModalKeyHandler = (ev) => {
        if (ev.key === 'Escape') {
            closeDeleteConfirmModal();
        }
    };
    document.addEventListener('keydown', _deleteModalKeyHandler);

    // Clicking the overlay (but not the modal content) cancels
    _deleteModalOverlayHandler = (ev) => {
        if (ev.target && ev.target.id === 'delete-confirm-modal') {
            closeDeleteConfirmModal();
        }
    };
    modal.addEventListener('click', _deleteModalOverlayHandler);
};

const closeDeleteConfirmModal = () => {
    const modal = document.getElementById('delete-confirm-modal');
    if (!modal) return;

    // hide modal and restore scroll immediately
    modal.classList.add('hidden');
    document.body.classList.remove('modal-open');

    // remove key handler if attached
    if (_deleteModalKeyHandler) {
        document.removeEventListener('keydown', _deleteModalKeyHandler);
        _deleteModalKeyHandler = null;
    }

    // remove overlay click handler if attached (fixed variable name)
    if (_deleteModalOverlayHandler) {
        modal.removeEventListener('click', _deleteModalOverlayHandler);
        _deleteModalOverlayHandler = null;
    }

    // clear pending deletion id
    pendingDeletionTaskId = null;
};

const handleConfirmDelete = () => {
    // explicit null check so an id of 0 (if it ever occurred) wouldn't be treated as "no task"
    if (pendingDeletionTaskId === null) {
        closeDeleteConfirmModal();
        showToast('No task selected for deletion.', 'error');
        return;
    }

    const origTask = tasks.find(t => Number(t.id) === Number(pendingDeletionTaskId));
    if (!origTask) {
        closeDeleteConfirmModal();
        showToast('Task not found. It may already have been removed.', 'error');
        return;
    }

    // Remove the task
    tasks = tasks.filter(t => Number(t.id) !== Number(pendingDeletionTaskId));
    saveTasksToStorage();

    showToast(`Task "${origTask.name}" deleted.`, 'success');

    closeDeleteConfirmModal();

    // Recalculate timeline/project bounds and re-render (fit exact).
    // fitTimelineToTasks(true) will call renderTimeline which calls renderTasks().
    adjustTimelineAfterDeletion();
    sendProjectUpdate();
};

const handleContextMenuAction = (action) => {
    const menu = document.getElementById('context-menu');

    // Prefer robust global id, fall back to attribute
    let taskId = null;
    if (contextMenuTaskId !== null && !isNaN(contextMenuTaskId)) {
        taskId = Number(contextMenuTaskId);
    } else {
        const attr = menu ? menu.getAttribute('data-task-id') : null;
        if (attr !== null) {
            const parsed = parseInt(attr);
            if (!isNaN(parsed)) taskId = parsed;
        }
    }

    // Hide the menu immediately
    if (menu) menu.classList.add('hidden');

    // clear global selection so subsequent actions are clean
    contextMenuTaskId = null;
    if (menu) menu.removeAttribute('data-task-id');

    if (!taskId && taskId !== 0) {
        showToast('Unable to determine selected task for that action.', 'error');
        return;
    }

    if (action === 'Edit') {
        openTaskModal(taskId);
    } else if (action === 'Clone') {
        const originalTask = tasks.find(t => Number(t.id) === Number(taskId));
        if (originalTask) {
            // clone must normalize: if original had progress 100 -> completed true
            const clone = Object.assign({}, originalTask);
            clone.id = Date.now();
            normalizeTask(clone);
            tasks.push(clone);
            saveTasksToStorage();
            showToast(`Task "${originalTask.name}" cloned successfully.`, 'success');
            // ensure UI updates even if timeline bounds don't change
            checkAndUpdateTimeline();
            sendProjectUpdate();
        } else {
            showToast('Original task not found — cannot clone.', 'error');
        }
    } else if (action === 'Delete') {
        // Open in-page confirmation modal (nicer UX than window.confirm)
        openDeleteConfirmModal(taskId);
    } else {
        showToast(`"${action}" feature is still in development.`, 'info');
    }
};

const renderProjectInfo = () => {
    // Access nested ProjectInfo object
    const projectInfo = currentProject.project || {};
    document.getElementById('project-name-display').textContent = projectInfo.name || 'Unnamed Project';
    document.getElementById('project-description-display').textContent = projectInfo.description || 'No description provided.';
};

// Tooltip utilities
function ensureTooltip() {
    if (!tooltipEl) {
        tooltipEl = document.getElementById('task-tooltip');
    }
}

function showTooltipAt(x, y, htmlContent) {
    ensureTooltip();
    if (!tooltipEl) return;
    tooltipEl.innerHTML = htmlContent || '';
    tooltipEl.classList.add('show');
    tooltipEl.classList.remove('hidden');
    tooltipEl.setAttribute('aria-hidden', 'false');

    // Positioning: prefer above the cursor, but keep within viewport
    const padding = 12;
    const vw = window.innerWidth;
    const vh = window.innerHeight;
    // Temporarily make visible to measure
    tooltipEl.style.left = '0px';
    tooltipEl.style.top = '0px';
    tooltipEl.style.maxWidth = '320px';

    const rect = tooltipEl.getBoundingClientRect();
    let left = x + 12; // slightly right of cursor
    let top = y - rect.height - 12; // above cursor

    // If it would go off right edge, shift left
    if (left + rect.width + padding > vw) {
        left = Math.max(padding, vw - rect.width - padding);
    }
    // If it would go above top, show below cursor instead
    if (top < padding) {
        top = y + 16; // below cursor
        // adjust caret position via CSS -- not necessary for this simple implementation
    }
    tooltipEl.style.left = `${left}px`;
    tooltipEl.style.top = `${top}px`;
}

function hideTooltip() {
    ensureTooltip();
    if (!tooltipEl) return;
    tooltipEl.classList.remove('show');
    // allow transition then hide
    setTimeout(() => {
        if (tooltipEl) {
            tooltipEl.classList.add('hidden');
            tooltipEl.setAttribute('aria-hidden', 'true');
        }
    }, 120);
}

// --- Tasks rendering (updated to attach hover tooltip) ---
const renderTasks = () => {
    const taskBarsContainer = document.getElementById('task-bars-container');
    const timelineContainer = document.getElementById('timeline-container');
    taskBarsContainer.innerHTML = '';

    ensureTooltip(); // ensure tooltip exists

    if (!timelineStartDate || timelineTotalDays <= 0) return;

    const BAR_HEIGHT_PX = 32;
    const BAR_MARGIN_PX = 8;
    const STACK_HEIGHT = BAR_HEIGHT_PX + BAR_MARGIN_PX;
    const BASE_TOP_OFFSET_PX = 64;

    const today = new Date();
    today.setHours(0, 0, 0, 0);

    tasks.forEach((task, index) => {
        // Safety normalize before rendering
        normalizeTask(task);

        const taskStart = new Date(task.startDate + 'T00:00:00');
        const taskEnd = new Date(task.endDate + 'T00:00:00');

        const startDayOffset = getDayDifference(timelineStartDate, taskStart);
        const endDayOffset = getDayDifference(timelineStartDate, taskEnd);

        const actualStartDay = Math.max(0, startDayOffset);
        const actualEndDay = Math.min(timelineTotalDays, endDayOffset);

        const visibleTaskDuration = actualEndDay - actualStartDay;

        const leftPercent = (actualStartDay / timelineTotalDays) * 100;
        const widthPercent = (visibleTaskDuration / timelineTotalDays) * 100;

        if (widthPercent > 0) {
            const taskBar = document.createElement('div');

            let bgColor, borderColor, labelSuffix, hoverColor = '';

            // Priority of styling:
            // 1) completed -> green
            // 2) overdue (taskEnd < today && not completed) -> overdue-red
            // 3) progress (0..99) -> indigo with percent suffix + overlay
            // 4) default -> indigo
            if (task.completed) {
                bgColor = 'bg-secondary-green/70';
                borderColor = 'border-primary-green';
                hoverColor = 'hover:bg-primary-green';
                labelSuffix = ' (Done)';
            } else if (taskEnd < today) {
                // Task ended in the past and not completed => overdue regardless of progress percent
                bgColor = 'bg-overdue-red/70';
                borderColor = 'border-red-800';
                hoverColor = 'hover:bg-red-800';
                labelSuffix = ' (OVERDUE)';
            } else if (task.progress !== null && typeof task.progress === 'number' && task.progress > 0) {
                // Show progress percent when >0 (but we already handled >=100 -> completed)
                bgColor = 'bg-indigo-500/60';
                borderColor = 'border-indigo-700';
                hoverColor = 'hover:bg-indigo-700';
                labelSuffix = ` (${Math.round(task.progress)}%)`;
            } else {
                bgColor = 'bg-indigo-500/70';
                borderColor = 'border-indigo-700';
                hoverColor = 'hover:bg-indigo-700';
            }

            taskBar.className = `absolute h-8 rounded-md shadow-lg transition-all duration-300 ${bgColor} ${hoverColor} ${borderColor} border cursor-pointer`;
            taskBar.setAttribute('data-task-id', String(task.id));
            taskBar.setAttribute('onclick', `openTaskModal(${task.id})`);
            taskBar.setAttribute('oncontextmenu', `showContextMenu(event, ${task.id})`);

            taskBar.style.left = `${leftPercent.toFixed(2)}%`;
            taskBar.style.width = `${widthPercent.toFixed(02)}%`;
            taskBar.style.top = `${index * STACK_HEIGHT}px`;

            const startMonth = taskStart.toLocaleString('en-US', { month: 'short' });
            const startDay = taskStart.getDate();
            const endMonth = taskEnd.toLocaleString('en-US', { month: 'short' });
            const endDay = taskEnd.getDate();

            // progress overlay if applicable (show inner fill representing percent)
            let progressHTML = '';
            if (!task.completed && typeof task.progress === 'number' && task.progress >= 0) {
                const pct = Math.max(0, Math.min(100, Math.round(task.progress)));
                // Do not show overlay if pct === 0
                if (pct > 0) {
                    // small darker overlay to indicate progress within the bar
                    progressHTML = `<div class="absolute left-0 top-0 h-full bg-primary-green/40" style="width: ${pct}%; pointer-events: none;"></div>`;
                }
            }

            taskBar.innerHTML = `
                            ${progressHTML}
                            <div class="h-full flex items-center px-4 relative z-10">
                                <span class="text-white text-xs font-semibold whitespace-nowrap text-ellipsis overflow-hidden">
                                    ${task.name}
                                    ${labelSuffix || ''}
                                </span>
                            </div>
                            <div class="task-start-display absolute top-0 -left-10 flex flex-col items-center w-12 z-10">
                                <span class="month-span text-xs font-bold text-gray-700">${startMonth}</span>
                                <span class="day-span text-xs text-gray-700">${startDay}</span>
                            </div>
                            <div class="task-end-display absolute top-0 -right-10 flex flex-col items-center w-12 z-10">
                                <span class="month-span text-xs font-bold text-gray-700">${endMonth}</span>
                                <span class="day-span text-xs text-gray-700">${endDay}</span>
                            </div>
                        `;

            // Attach hover handlers for tooltip

            // mouseenter -> show tooltip
            taskBar.addEventListener('mouseenter', (ev) => {
                // FIX: specific check to stop tooltip if we are currently resizing
                if (isResizing) return;

                // If context menu or modal open, do not show
                const ctx = document.getElementById('context-menu');
                if (ctx && !ctx.classList.contains('hidden')) return;

                const descr = task.description || 'No description provided.';
                const safeHtml = escapeHtml(descr);
                showTooltipAt(ev.clientX, ev.clientY, safeHtml);
            });

            // mousemove -> reposition tooltip
            taskBar.addEventListener('mousemove', (ev) => {
                // FIX: specific check to stop tooltip if we are currently resizing
                if (isResizing) return;

                const ctx = document.getElementById('context-menu');
                if (ctx && !ctx.classList.contains('hidden')) {
                    hideTooltip();
                    return;
                }
                showTooltipAt(ev.clientX, ev.clientY, escapeHtml(task.description || 'No description provided.'));
            });

            // mouseleave -> hide tooltip
            taskBar.addEventListener('mouseleave', () => {
                hideTooltip();
            });

            // --- NEW: Add Resize Handles ---

            // Create and append Left Handle
            const leftHandle = document.createElement('div');
            leftHandle.className = 'resize-handle resize-handle-left';
            leftHandle.addEventListener('mousedown', (ev) => onHandleMouseDown(ev, task.id, 'start'));
            taskBar.appendChild(leftHandle);

            // Create and append Right Handle
            const rightHandle = document.createElement('div');
            rightHandle.className = 'resize-handle resize-handle-right';
            rightHandle.addEventListener('mousedown', (ev) => onHandleMouseDown(ev, task.id, 'end'));
            taskBar.appendChild(rightHandle);

            taskBarsContainer.appendChild(taskBar);
        }
    });

    if (tasks.length > 0) {
        const requiredHeight = BASE_TOP_OFFSET_PX + (tasks.length * STACK_HEIGHT) + 40;
        timelineContainer.style.minHeight = `${requiredHeight}px`;
    } else {
        timelineContainer.style.minHeight = '60vh';
    }
};

// Utility to escape text into safe HTML (very simple)
function escapeHtml(text) {
    if (text === undefined || text === null) return '';
    return String(text)
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#039;')
        .replace(/\n/g, '<br>');
}

// --- Main timeline render (unchanged except month boundary shift) ---
const renderTimeline = (startDateStr, endDateStr) => {
    const tickContainer = document.getElementById('daily-ticks-container');
    const monthLabelsContainer = document.getElementById('month-labels-container');
    const progressFill = document.getElementById('progress-fill');
    const todayMarker = document.getElementById('today-marker');
    const todayLabel = document.getElementById('today-label');
    const startMarkerLabel = document.getElementById('start-marker-label');
    const endMarkerLabel = document.getElementById('end-marker-label');

    progressFill.addEventListener('click', (ev) => {
        ev.preventDefault();
        openScaleModal();
    });

    tickContainer.innerHTML = '';
    monthLabelsContainer.innerHTML = '';
    progressFill.style.width = '0%';
    todayMarker.style.left = '0%';
    todayMarker.classList.add('hidden');

    const startDate = new Date(startDateStr + 'T00:00:00');
    const endDate = new Date(endDateStr + 'T00:00:00');
    const today = new Date();
    today.setHours(0, 0, 0, 0);

    const totalDays = getDayDifference(startDate, endDate);

    if (totalDays <= 0 || isNaN(startDate.getTime()) || isNaN(endDate.getTime())) {
        showToast('Invalid date range. Ensure the Start Date is before the End Date.', 'error');
        timelineStartDate = null;
        timelineEndDate = null;
        timelineTotalDays = 0;
        renderTasks();
        return;
    }

    timelineStartDate = startDate;
    timelineEndDate = endDate;
    timelineTotalDays = totalDays;

    const formatter = new Intl.DateTimeFormat('en-US', { month: 'short', day: 'numeric', year: 'numeric' });
    const monthFormatter = new Intl.DateTimeFormat('en-US', { month: 'short' });

    startMarkerLabel.innerHTML = `<p class="text-xs font-semibold text-primary-green mt-1">${formatter.format(startDate)}</p>`;
    endMarkerLabel.innerHTML = `<p class="text-xs font-semibold text-gray-900 mt-1">${formatter.format(endDate)}</p>`;

    const monthBoundaries = {};

    for (let i = 0; i <= totalDays; i++) {
        const currentDay = new Date(startDate);
        currentDay.setDate(startDate.getDate() + i);

        const monthKey = `${currentDay.getFullYear()}-${String(currentDay.getMonth() + 1).padStart(2, '0')}`;
        const monthName = monthFormatter.format(currentDay);

        const positionPercent = (i / totalDays) * 100;

        if (!monthBoundaries[monthKey]) {
            monthBoundaries[monthKey] = {
                name: monthName,
                startDayIndex: i,
                endDayIndex: i
            };
        }

        monthBoundaries[monthKey].endDayIndex = i;

        if (i > 0 && i < totalDays) {
            const calendarDay = currentDay.getDate();
            const position = positionPercent;

            let className = 'daily-tick';
            let labelHTML = '';

            if (i % 5 === 0) {
                className = 'labeled-tick';
                labelHTML = `<div class="absolute -top-4 left-1/2 transform -translate-x-1/2 w-10 text-center text-xs font-bold text-gray-700 z-20">${calendarDay}</div>`;
            }

            const tickMarker = document.createElement('div');
            tickMarker.className = `vertical-tick-marker`;
            tickMarker.style.left = `${position}%`;
            tickMarker.innerHTML = labelHTML + `<div class="${className}"></div>`;

            tickContainer.appendChild(tickMarker);
        }
    }

    const monthKeys = Object.keys(monthBoundaries).sort();

    for (let i = 0; i < monthKeys.length; i++) {
        const currentMonthKey = monthKeys[i];
        const currentMonth = monthBoundaries[currentMonthKey];

        // Shift month boundaries one day to the right for visual alignment
        let monthStartDayIndex;
        if (i === 0) {
            // keep first month start at 0 so we don't create an empty gap at timeline start
            monthStartDayIndex = 0;
        } else {
            const previousMonthKey = monthKeys[i - 1];
            // start one day after the previous month's end
            monthStartDayIndex = Math.min(totalDays, monthBoundaries[previousMonthKey].endDayIndex + 1);
        }

        // end boundary should also be one day to the right for correct visual placement
        const monthEndDayIndex = Math.min(totalDays, currentMonth.endDayIndex + 1);

        const leftPercent = (monthStartDayIndex / totalDays) * 100;
        const rightPercent = (monthEndDayIndex / totalDays) * 100;

        const widthPercent = rightPercent - leftPercent;

        if (widthPercent <= 0) continue;

        const monthLabel = document.createElement('div');
        monthLabel.className = 'absolute bg-gray-200 h-4 flex items-center justify-center text-xs font-bold text-gray-900 px-2 border border-gray-500 z-30';

        monthLabel.style.left = `${leftPercent.toFixed(2)}%`;
        monthLabel.style.width = `${widthPercent.toFixed(2)}%`;

        monthLabel.textContent = currentMonth.name;

        monthLabelsContainer.appendChild(monthLabel);
    }

    const todayDayNumber = getDayDifference(startDate, today);

    if (todayDayNumber >= 0 && todayDayNumber <= totalDays) {
        const progressPercent = (todayDayNumber / totalDays) * 100;
        progressFill.style.width = `${progressPercent.toFixed(2)}%`;
        todayMarker.style.left = `${progressPercent.toFixed(02)}%`;
        todayMarker.classList.remove('hidden');
        todayLabel.textContent = `Today: ${formatter.format(today)}`;
    } else if (todayDayNumber < 0) {
        progressFill.style.width = '0%';
        todayMarker.style.left = '0%';
        todayMarker.classList.remove('hidden');
        todayLabel.textContent = `Before Start (${formatter.format(today)})`;
    } else {
        progressFill.style.width = '100%';
        todayMarker.style.left = '100%';
        todayMarker.classList.remove('hidden');
        todayLabel.textContent = `After End (${formatter.format(today)})`;
    }

    renderTasks();
};

// --- New: sort tasks by their actual start date (ascending) and re-render ---
const sortTasksByStartDate = () => {
    tasks.sort((a, b) => {
        const da = new Date(a.startDate + 'T00:00:00');
        const db = new Date(b.startDate + 'T00:00:00');
        if (da < db) return -1;
        if (da > db) return 1;
        // tie-breaker by id to keep deterministic order
        return Number(a.id) - Number(b.id);
    });
    saveTasksToStorage();
    // Re-render timeline & tasks (use full render to recalc positions reliably)
    const projectInfo = currentProject.project || {};
    renderTimeline(projectInfo.startDate, projectInfo.endDate);
    showToast('Tasks sorted by start date (earliest at top).', 'success');
};

// --- Event listeners & initial bootstrapping ---
document.addEventListener('DOMContentLoaded', async () => {
    // Load persisted state
    let bootstrapped = false;
    try {
        if (typeof window !== 'undefined' && window.__INITIAL_DATA__) {
            const initial = window.__INITIAL_DATA__;
            if (initial && typeof initial === 'object') {
                // The server sends the full Project structure
                // Check if it's a full Project (has id, tasks, project properties)
                if (initial.id && Array.isArray(initial.tasks) && initial.project) {
                    // Full Project structure from server - use it directly
                    tasks = initial.tasks.map(t => { const c = Object.assign({}, t); normalizeTask(c); return c; });
                    currentProject = Object.assign({}, initial);
                    console.log('[Bootstrap] Loaded full Project from server, id:', currentProject.id, '. Name: ', currentProject.project.name);
                    // IMPORTANT: Always save server's project to localStorage to keep them in sync
                    saveTasksToStorage();
                    saveProjectToStorage();
                    bootstrapped = true;
                } else if (Array.isArray(initial.tasks) && initial.project) {
                    // Legacy format (partial) - construct full Project
                    // CONSIDER TO DELETE THAT
                    tasks = initial.tasks.map(t => { const c = Object.assign({}, t); normalizeTask(c); return c; });
                    currentProject = {
                        id: initial.id || generateGuid(),
                        tasks: tasks,
                        project: Object.assign({}, initial.project),
                        lastUpdatedTask: initial.lastUpdatedTask || null,
                        clientTimestamp: initial.clientTimestamp || new Date().toISOString()
                    };
                    console.warn('[Bootstrap] Server sent partial format, constructed full Project');
                    saveTasksToStorage();
                    saveProjectToStorage();
                    bootstrapped = true;
                }
            }
        }
    } catch (e) {
        console.error('[Bootstrap] Error loading initial data:', e);
    }

    if (!bootstrapped) {
        loadProjectFromStorage();
        loadTasksFromStorage();
    }

    renderProjectInfo();
    const projectInfo = currentProject.project || {};
    renderTimeline(projectInfo.startDate, projectInfo.endDate);
    sendProjectUpdate();

    // Task Modal listeners
    document.getElementById('open-task-modal').addEventListener('click', () => openTaskModal(null));
    document.getElementById('cancel-task').addEventListener('click', closeTaskModal);
    document.getElementById('save-task-ok').addEventListener('click', saveTask);

    // Project Modal listeners
    document.getElementById('open-project-modal').addEventListener('click', openProjectModal);
    document.getElementById('project-modal-cancel').addEventListener('click', closeProjectModal);
    document.getElementById('project-modal-ok').addEventListener('click', handleProjectModalOk);

    const tabCur = document.getElementById('tab-project-current');
    if (tabCur) tabCur.addEventListener('click', () => setProjectModalTab('current'));
    const tabNew = document.getElementById('tab-project-new');
    if (tabNew) tabNew.addEventListener('click', () => setProjectModalTab('new'));
    const tabOpen = document.getElementById('tab-project-open');
    if (tabOpen) tabOpen.addEventListener('click', () => setProjectModalTab('open'));

    const refreshListBtn = document.getElementById('refresh-projects-list');
    if (refreshListBtn) refreshListBtn.addEventListener('click', () => ensureProjectsListLoadedAndRender(true));

    // QUICK SCALE modal listeners (progress-line click / small modal)
    const progressLine = document.getElementById('progress-line');
    if (progressLine) {
        // open quick-scale modal when user clicks the progress line
        progressLine.addEventListener('click', (ev) => {
            ev.preventDefault();
            openScaleModal();
        });
    }
    const scaleCancel = document.getElementById('scale-modal-cancel');
    const scaleOk = document.getElementById('scale-modal-ok');
    if (scaleCancel) scaleCancel.addEventListener('click', closeScaleModal);
    if (scaleOk) scaleOk.addEventListener('click', handleScaleModalOk);

    // Delete-confirm modal listeners
    const deleteCancel = document.getElementById('delete-confirm-cancel');
    const deleteOk = document.getElementById('delete-confirm-ok');
    if (deleteCancel) deleteCancel.addEventListener('click', closeDeleteConfirmModal);
    if (deleteOk) deleteOk.addEventListener('click', handleConfirmDelete);

    // Scale button - wire to fitTimelineToTasks (fit exact to tasks)
    const scaleBtn = document.getElementById('open-scale-modal');
    if (scaleBtn) {
        // remove any inline onclick to avoid double toasts
        scaleBtn.removeAttribute('onclick');
        scaleBtn.addEventListener('click', (ev) => {
            ev.preventDefault();
            // Fit timeline exactly to current task bounds (shrinkAllowed = true)
            fitTimelineToTasks(true);
            sendProjectUpdate();
        });
    }

    // Sort button - wire to sortTasksByStartDate
    const sortBtn = document.getElementById('open-sort-modal');
    if (sortBtn) {
        sortBtn.addEventListener('click', (ev) => {
            ev.preventDefault();
            sortTasksByStartDate();
            sendProjectUpdate();
        });
    }

    // Hide context menu when clicking anywhere else
    document.addEventListener('click', (e) => {
        const menu = document.getElementById('context-menu');
        if (menu && !menu.contains(e.target)) {
            menu.classList.add('hidden');
            // clear global selection on outside click as well
            contextMenuTaskId = null;
            if (menu) menu.removeAttribute('data-task-id');
        }
    });

    // Hide context menu on scroll and hide tooltip on scroll
    window.addEventListener('scroll', () => {
        const menu = document.getElementById('context-menu');
        if (menu) {
            menu.classList.add('hidden');
            contextMenuTaskId = null;
            menu.removeAttribute('data-task-id');
        }
        hideTooltip();
    });

    // Hide tooltip when resizing
    window.addEventListener('resize', hideTooltip);
});