// Global variable to track current mode
let isRegistrationMode = false;
const baseUrl = 'http://localhost:5146'; // Base URL for the API

// Keep a timer handle on window so other modules can clear it if needed
window.statusTimer = window.statusTimer || null; // ???

/**
 * Displays a success or error message in the dedicated status toast.
 * If called with no message (or empty string), it clears/hides the toast immediately.
 * @param {string} message - The message to display. If falsy, toast will be hidden.
 * @param {('success'|'error')} [type='success'] - The type of message to display.
 */
function displayStatus(message = '', type = 'success') {
    const toast = document.getElementById('statusToast');
    const messageEl = document.getElementById('statusMessage');

    if (!toast || !messageEl) return;

    // Clear any existing timer and classes
    clearTimeout(window.statusTimer);
    toast.classList.remove('status-success', 'status-error');
    messageEl.textContent = '';

    if (!message) {
        // Hide immediately if no message provided
        toast.classList.add('status-hidden');
        return;
    }

    // Set content and style
    messageEl.textContent = message;
    toast.classList.remove('status-hidden');
    toast.classList.add(type === 'error' ? 'status-error' : 'status-success');

    // Auto-hide after 4 seconds
    window.statusTimer = setTimeout(() => {
        toast.classList.add('status-hidden');
        messageEl.textContent = '';
    }, 4000);
}

// Toggle between login and registration modes
function toggleMode() {
    isRegistrationMode = !isRegistrationMode;

    const nameGroup = document.getElementById('nameGroup');
    const confirmPasswordGroup = document.getElementById('confirmPasswordGroup');
    const passwordRequirements = document.getElementById('passwordRequirements');
    const pageTitle = document.getElementById('page-title');
    const modeToggleBtn = document.getElementById('modeToggleBtn');
    const mainSubmitBtn = document.getElementById('mainSubmit');

    // Clear any existing status message when switching modes
    displayStatus();

    if (isRegistrationMode) {
        // Switch to registration mode
        nameGroup.classList.remove('hidden');
        confirmPasswordGroup.classList.remove('hidden');
        passwordRequirements.classList.remove('hidden');
        pageTitle.textContent = 'User Registration';
        modeToggleBtn.textContent = 'Switch to Login';
        mainSubmitBtn.textContent = 'Register';

        // Add required attributes for registration
        document.getElementById('name').required = true;
        document.getElementById('confirmPassword').required = true;

        // Focus on name field
        document.getElementById('name').focus();

        // Initialize registration validation
        validateForm();
    } else {
        // Switch to login mode
        nameGroup.classList.add('hidden');
        confirmPasswordGroup.classList.add('hidden');
        passwordRequirements.classList.add('hidden');
        pageTitle.textContent = 'User Login';
        modeToggleBtn.textContent = 'Switch to Registration';
        mainSubmitBtn.textContent = 'Login';

        // Remove required attributes for login
        document.getElementById('name').required = false;
        document.getElementById('confirmPassword').required = false;

        // Initialize login validation
        validateLoginForm();
    }
}

/**
 * Handles the form submission (either registration or login).
 */
function submitForm(event) {
    event.preventDefault(); // Prevent default form submission

    // Determine which validation function to call
    const isValid = isRegistrationMode ? validateForm(true) : validateLoginForm(true);

    if (isValid) {
        if (isRegistrationMode) {
            registerUser();
        } else {
            loginUser();
        }
    } else {
        displayStatus('Please correct the validation errors before submitting.', 'error');
    }
}

/**
 * Sends registration data as a JSON body to the /register endpoint.
 */
async function registerUser() {
    const name = document.getElementById('name').value.trim();
    const email = document.getElementById('email').value.trim();
    const password = document.getElementById('password').value;

    const userData = {
        name,
        email,
        password,
        // The server requires these, so we send default values if the client side doesn't use them.
        settings: "{}",
        pages: "{}",
        links: "{}"
    };

    const url = `${baseUrl}/register`;

    try {
        console.log('Sending registration request to:', url);
        const response = await fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(userData) // Send data as JSON body
        });

        if (response.ok) {
            const newUser = await response.json();
            // store session id for subsequent authenticated calls
            try {
                if (newUser && newUser.sessionId) {
                    localStorage.setItem('session_id_v1', String(newUser.sessionId));
                }
            } catch (e) { }
            displayStatus(`Registration successful! Welcome, ${newUser.name || ''}. Redirecting...`, 'success');
            // Redirect to TaskManager (include sessionId as query for initial load)
            const sid = newUser && newUser.sessionId ? String(newUser.sessionId) : '';
            window.location.href = `/getProject${sid ? (`?sessionId=${encodeURIComponent(sid)}`) : ''}`;
        } else {
            let errorText = 'Please try again.';
            try {
                const errorData = await response.json();
                errorText = errorData.details ? errorData.details.join(', ') : (errorData.error || errorText);
            } catch (e) {
                // ignore parse errors
            }
            displayStatus(`Registration failed: ${errorText}`, 'error');
        }
    } catch (error) {
        console.error('Error:', error);
        displayStatus('Registration failed. The server may not be reachable.', 'error');
    }
}

/**
 * Sends login data as a JSON body to the /login endpoint.
 */
async function loginUser() {
    const email = document.getElementById('email').value.trim();
    const password = document.getElementById('password').value;

    const loginData = { email, password };
    const url = `${baseUrl}/login`;

    try {
        console.log('Sending login request to:', url);
        const response = await fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(loginData),
            credentials: 'include' // <--- THIS IS CRITICAL FOR SENDING COOKIES
        });

        // Server-side login error info
        if (response.ok) {

            const okData = await response.json();
            displayStatus(okData.success, 'success');
            window.location.href = `${baseUrl}/getProject`;
            
        }
        else {
            let errorText = 'Invalid credentials or server issue.';
            try {
                const errorData = await response.json();
                errorText = errorData.error || errorText;
            } catch (e) { }

            displayStatus(`Login failed: ${errorText}`, 'error');
        }


    } catch (error) {
        console.error('Error:', error);
        displayStatus('Login failed. The server may not be reachable.', 'error');
    }
}

// --- Validation Functions ---

// Password regex (matching the one in User.cs)
const passwordRegex = /^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[^A-Za-z0-9]).{8,128}$/;

function validateField(id, regex, minLength, maxLength) {
    const input = document.getElementById(id);
    const error = document.getElementById(`${id}-error`);
    const success = document.getElementById(`${id}-success`);

    if (!input || !error || !success) return true; // Skip if elements are not found

    let isValid = true;
    const value = input.value.trim();

    // Length check
    if (minLength && value.length < minLength) {
        isValid = false;
    }
    if (maxLength && value.length > maxLength) {
        isValid = false;
    }

    // Regex check (if provided)
    if (regex && value.length > 0 && !regex.test(value)) {
        isValid = false;
    }

    if (isValid) {
        error.classList.remove('visible');
        success.classList.add('visible');
    } else {
        error.classList.add('visible');
        success.classList.remove('visible');
    }

    return isValid;
}

function validateForm(isSubmit = false) {
    let isValid = true;

    // Validate Name (3-30 chars, required in registration mode)
    if (isRegistrationMode) {
        isValid &= validateField('name', null, 3, 30);
    }

    // Validate Email
    isValid &= validateField('email', /.+@.+\..+/, null, 255);

    // Validate Password
    isValid &= validateField('password', passwordRegex, 8, 128);

    // Validate Confirm Password (only in registration mode)
    if (isRegistrationMode) {
        const password = document.getElementById('password').value;
        const confirmPassword = document.getElementById('confirmPassword').value;

        const confirmError = document.getElementById('confirmPassword-error');
        const confirmSuccess = document.getElementById('confirmPassword-success');

        const match = password === confirmPassword && password.length > 0 && passwordRegex.test(password);

        if (confirmError && confirmSuccess) {
            if (match) {
                confirmError.classList.remove('visible');
                confirmSuccess.classList.add('visible');
            } else {
                confirmError.classList.add('visible');
                confirmSuccess.classList.remove('visible');
                isValid = false;
            }
        }
    }

    const submitBtn = document.getElementById('mainSubmit');
    if (submitBtn) {
        // Disable the button unless it's a submission attempt and all fields are valid
        submitBtn.disabled = !isValid && !isSubmit;
    }

    return !!isValid;
}

function validateLoginForm(isSubmit = false) {
    let isValid = true;

    // Validate Email
    isValid &= validateField('email', /.+@.+\..+/, null, 255);

    // Validate Password
    isValid &= validateField('password', null, 8, 128); // Minimal check for login

    const submitBtn = document.getElementById('mainSubmit');
    if (submitBtn) {
        submitBtn.disabled = !isValid && !isSubmit;
    }

    return !!isValid;
}

// --- Event Listeners and Initialization ---

function setupValidationListeners() {
    const form = document.getElementById('mainForm');
    if (form) {
        const inputs = form.querySelectorAll('input');
        inputs.forEach(input => {
            input.addEventListener('input', () => {
                if (isRegistrationMode) {
                    validateForm();
                } else {
                    validateLoginForm();
                }
            });
        });
    }
}

function initializePage() {
    setupValidationListeners();
    // Start with login form visible and validated
    validateLoginForm();
    // Ensure toast is hidden initially
    displayStatus();
}

// Initialize the page
document.addEventListener('DOMContentLoaded', initializePage);