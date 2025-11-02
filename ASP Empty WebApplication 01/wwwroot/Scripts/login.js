
// Global variable to track current mode
let isRegistrationMode = false;

// Toggle between login and registration modes
function toggleMode() {
    isRegistrationMode = !isRegistrationMode;

    const nameGroup = document.getElementById('nameGroup');
    const confirmPasswordGroup = document.getElementById('confirmPasswordGroup');
    const passwordRequirements = document.getElementById('passwordRequirements');
    const pageTitle = document.getElementById('page-title');
    const modeToggleBtn = document.getElementById('modeToggleBtn');
    const mainSubmitBtn = document.getElementById('mainSubmit');

    if (isRegistrationMode) {
        // Switch to registration mode
        nameGroup.classList.remove('hidden');
        confirmPasswordGroup.classList.remove('hidden');
        passwordRequirements.classList.remove('hidden');
        viewRegistrationsBtn.classList.remove('hidden');
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
        passwordRequirements.classList.remove('hidden'); // Show password requirements in login mode too
        viewRegistrationsBtn.classList.add('hidden');
        pageTitle.textContent = 'User Login';
        modeToggleBtn.textContent = 'Switch to Registration';
        mainSubmitBtn.textContent = 'Login';

        // Remove required attributes for login
        document.getElementById('name').required = false;
        document.getElementById('confirmPassword').required = false;

        // Focus on email field
        document.getElementById('email').focus();

        // Clear registration-specific validation
        clearRegistrationValidation();

        // Initialize login validation
        validateForm();
    }
}

// Clear registration-specific validation states
function clearRegistrationValidation() {
    document.getElementById('name-error').style.display = 'none';
    document.getElementById('name-success').style.display = 'none';
    document.getElementById('confirmPassword-error').style.display = 'none';
    document.getElementById('confirmPassword-success').style.display = 'none';
    document.getElementById('passwordRequirements').classList.add('hidden');
}

// Login validation functions
function validateLoginEmail() {
    const emailInput = document.getElementById('email');
    const emailError = document.getElementById('email-error');
    const emailSuccess = document.getElementById('email-success');

    // Clear any previous custom validity
    emailInput.setCustomValidity('');

    if (emailInput.value.length === 0) {
        emailError.style.display = 'none';
        emailSuccess.style.display = 'none';
        return false;
    }

    if (emailInput.value.length > 255) {
        emailInput.setCustomValidity('Email address is too long (maximum 255 characters).');
        emailError.textContent = 'Email address is too long (maximum 255 characters).';
        emailError.style.display = 'block';
        emailSuccess.style.display = 'none';
        return false;
    }

    // Use a more comprehensive email regex pattern
    const emailRegex = /^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$/;

    if (!emailRegex.test(emailInput.value)) {
        emailInput.setCustomValidity('Please enter a valid email address.');
        emailError.textContent = 'Please enter a valid email address.';
        emailError.style.display = 'block';
        emailSuccess.style.display = 'none';
        return false;
    } else {
        emailError.style.display = 'none';
        emailSuccess.style.display = 'block';
        return true;
    }
}

function validateLoginPassword() {
    const passwordInput = document.getElementById('password');
    const passwordError = document.getElementById('password-error');
    const passwordSuccess = document.getElementById('password-success');

    const password = passwordInput.value;

    // Clear any previous custom validity
    passwordInput.setCustomValidity('');

    if (password.length === 0) {
        passwordError.textContent = 'Password is required.';
        passwordError.style.display = 'block';
        passwordSuccess.style.display = 'none';
        return false;
    }

    // Check password requirements (same as registration)
    const requirements = {
        length: password.length >= 8 && password.length <= 128,
        uppercase: /[A-Z]/.test(password),
        lowercase: /[a-z]/.test(password),
        digit: /[0-9]/.test(password),
        special: /[!@#$%^&*()_+\-=\[\]{};':"\\|,.<>\/?]/.test(password)
    };

    const failedRequirements = [];
    if (!requirements.length) failedRequirements.push('8-128 characters');
    if (!requirements.uppercase) failedRequirements.push('one uppercase letter');
    if (!requirements.lowercase) failedRequirements.push('one lowercase letter');
    if (!requirements.digit) failedRequirements.push('one number');
    if (!requirements.special) failedRequirements.push('one special character');

    if (failedRequirements.length > 0) {
        const errorMessage = `Password must contain: ${failedRequirements.join(', ')}.`;
        passwordInput.setCustomValidity(errorMessage);
        passwordError.textContent = errorMessage;
        passwordError.style.display = 'block';
        passwordSuccess.style.display = 'none';
        return false;
    } else {
        passwordError.style.display = 'none';
        passwordSuccess.style.display = 'block';
        return true;
    }
}

// Registration validation functions
function validateName() {
    const nameInput = document.getElementById('name');
    const nameError = document.getElementById('name-error');
    const nameSuccess = document.getElementById('name-success');

    if (nameInput.value.length < 3 || nameInput.value.length > 30) {
        nameInput.setCustomValidity('Name must be between 3 and 30 characters long.');
        nameError.style.display = 'block';
        nameSuccess.style.display = 'none';
        return false;
    } else {
        nameInput.setCustomValidity('');
        nameError.style.display = 'none';
        nameSuccess.style.display = 'block';
        return true;
    }
}

function validateEmail() {
    const emailInput = document.getElementById('email');
    const emailError = document.getElementById('email-error');
    const emailSuccess = document.getElementById('email-success');

    // Clear any previous custom validity
    emailInput.setCustomValidity('');

    if (emailInput.value.length === 0) {
        emailError.style.display = 'none';
        emailSuccess.style.display = 'none';
        return false;
    }

    if (emailInput.value.length > 255) {
        emailInput.setCustomValidity('Email address is too long (maximum 255 characters).');
        emailError.textContent = 'Email address is too long (maximum 255 characters).';
        emailError.style.display = 'block';
        emailSuccess.style.display = 'none';
        return false;
    }

    // Use a more comprehensive email regex pattern
    const emailRegex = /^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$/;

    if (!emailRegex.test(emailInput.value)) {
        emailInput.setCustomValidity('Please enter a valid email address.');
        emailError.textContent = 'Please enter a valid email address.';
        emailError.style.display = 'block';
        emailSuccess.style.display = 'none';
        return false;
    } else {
        emailError.style.display = 'none';
        emailSuccess.style.display = 'block';
        return true;
    }
}

function validatePassword() {
    const passwordInput = document.getElementById('password');
    const passwordError = document.getElementById('password-error');
    const passwordSuccess = document.getElementById('password-success');

    const password = passwordInput.value;

    // Clear any previous custom validity
    passwordInput.setCustomValidity('');

    if (password.length === 0) {
        passwordError.textContent = 'Password is required.';
        passwordError.style.display = 'block';
        passwordSuccess.style.display = 'none';
        return false;
    }

    // Check password requirements
    const requirements = {
        length: password.length >= 8 && password.length <= 128,
        uppercase: /[A-Z]/.test(password),
        lowercase: /[a-z]/.test(password),
        digit: /[0-9]/.test(password),
        special: /[!@#$%^&*()_+\-=\[\]{};':"\\|,.<>\/?]/.test(password)
    };

    const failedRequirements = [];
    if (!requirements.length) failedRequirements.push('8-128 characters');
    if (!requirements.uppercase) failedRequirements.push('one uppercase letter');
    if (!requirements.lowercase) failedRequirements.push('one lowercase letter');
    if (!requirements.digit) failedRequirements.push('one number');
    if (!requirements.special) failedRequirements.push('one special character');

    if (failedRequirements.length > 0) {
        const errorMessage = `Password must contain: ${failedRequirements.join(', ')}.`;
        passwordInput.setCustomValidity(errorMessage);
        passwordError.textContent = errorMessage;
        passwordError.style.display = 'block';
        passwordSuccess.style.display = 'none';
        return false;
    } else {
        passwordError.style.display = 'none';
        passwordSuccess.style.display = 'block';
        return true;
    }
}

function validateConfirmPassword() {
    const passwordInput = document.getElementById('password');
    const confirmPasswordInput = document.getElementById('confirmPassword');
    const confirmPasswordError = document.getElementById('confirmPassword-error');
    const confirmPasswordSuccess = document.getElementById('confirmPassword-success');

    if (confirmPasswordInput.value !== passwordInput.value) {
        confirmPasswordInput.setCustomValidity('Passwords do not match.');
        confirmPasswordError.style.display = 'block';
        confirmPasswordSuccess.style.display = 'none';
        return false;
    } else {
        confirmPasswordInput.setCustomValidity('');
        confirmPasswordError.style.display = 'none';
        confirmPasswordSuccess.style.display = 'block';
        return true;
    }
}

function validateLoginForm() {
    const emailValid = validateLoginEmail();
    const passwordValid = validateLoginPassword();

    const submitButton = document.getElementById('mainSubmit');
    submitButton.disabled = !(emailValid && passwordValid);

    return emailValid && passwordValid;
}

function validateRegistrationForm() {
    const nameValid = validateName();
    const emailValid = validateEmail();
    const passwordValid = validatePassword();
    const confirmPasswordValid = validateConfirmPassword();

    const submitButton = document.getElementById('mainSubmit');
    submitButton.disabled = !(nameValid && emailValid && passwordValid && confirmPasswordValid);

    return nameValid && emailValid && passwordValid && confirmPasswordValid;
}

function validateForm() {
    if (isRegistrationMode) {
        return validateRegistrationForm();
    } else {
        return validateLoginForm();
    }
}

// Event listeners
document.getElementById('name').addEventListener('input', function () {
    if (isRegistrationMode) {
        validateName();
        validateForm(); // Update button state
    }
});
document.getElementById('name').addEventListener('blur', function () {
    if (isRegistrationMode) {
        validateName();
        validateForm(); // Update button state
    }
});

document.getElementById('email').addEventListener('input', function () {
    if (isRegistrationMode) {
        validateEmail();
    } else {
        validateLoginEmail();
    }
    validateForm(); // Update button state
});
document.getElementById('email').addEventListener('blur', function () {
    if (isRegistrationMode) {
        validateEmail();
    } else {
        validateLoginEmail();
    }
    validateForm(); // Update button state
});

document.getElementById('password').addEventListener('input', function () {
    if (isRegistrationMode) {
        validatePassword();
        validateConfirmPassword(); // Re-validate confirm password when password changes
    } else {
        validateLoginPassword();
    }
    validateForm(); // Update button state
});
document.getElementById('password').addEventListener('blur', function () {
    if (isRegistrationMode) {
        validatePassword();
    } else {
        validateLoginPassword();
    }
    validateForm(); // Update button state
});

document.getElementById('confirmPassword').addEventListener('input', function () {
    if (isRegistrationMode) {
        validateConfirmPassword();
        validateForm(); // Update button state
    }
});
document.getElementById('confirmPassword').addEventListener('blur', function () {
    if (isRegistrationMode) {
        validateConfirmPassword();
        validateForm(); // Update button state
    }
});

// Main form submission function
async function submitForm(event) {
    event.preventDefault();

    if (!validateForm()) {
        return false;
    }

    if (isRegistrationMode) {
        await submitRegistration();
    } else {
        await submitLogin();
    }
}

// Login submission
async function submitLogin() {
    const email = document.getElementById('email').value;
    const password = document.getElementById('password').value;

    // URL encode the parameters to handle special characters
    const encodedEmail = encodeURIComponent(email);
    const encodedPassword = encodeURIComponent(password);

    // Construct the URL with route parameters
    const url = `http://localhost:5146/login/${encodedEmail}/${encodedPassword}`;

    try {
        console.log('Sending login request to:', url);
        // Fetch to the constructed URL
        const response = await fetch(url, {
            method: 'POST', // Use POST method as for registration
            headers: {
                'Content-Type': 'application/json',
            }
        });

        if (response.ok) {
            const userData = await response.json();
            alert(`Login successful! Welcome, User ID: ${userData.id}`);
            // Optionally clear the form
            document.getElementById('mainForm').reset();
            validateForm(); // Reset button state
        } else {
            // Check if the response body is available and parse for error message
            const errorData = await response.json();
            const errorMessage = errorData.error || 'Invalid credentials or server error. Please try again.';
            alert(`Login failed: ${errorMessage}`);
        }
    } catch (error) {
        console.error('Login error:', error);
        alert('Login failed. Could not connect to the server. Please check the network.');
    }
}

// Registration submission
async function submitRegistration() {
    // Get form values
    const name = document.getElementById('name').value;
    const email = document.getElementById('email').value;
    const password = document.getElementById('password').value;

    // URL encode the parameters to handle special characters
    const encodedName = encodeURIComponent(name);
    const encodedEmail = encodeURIComponent(email);
    const encodedPassword = encodeURIComponent(password);

    // Construct the URL with route parameters
    const url = `http://localhost:5146/register/${encodedName}/${encodedEmail}/${encodedPassword}`;

    try {
        console.log('Sending registration request to:', url);
        // Fetch to the constructed URL
        const response = await fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            }
        });

        if (response.ok) {
            const newUser = await response.json();
            alert(`Registration successful! User ID: ${newUser.id}. ${newUser.message}`);
            // Optionally clear the form
            document.getElementById('mainForm').reset();
            validateForm(); // Reset button state
        } else {
            const errorData = await response.json();
            alert(`Registration failed: ${errorData.error || 'Please try again.'}`);
        }
    } catch (error) {
        console.error('Error:', error);
        alert('Registration failed. Please try again.');
    }
}

// Initial setup - start with login form visible
function initializePage() {
    // Initialize login validation
    validateLoginForm();
}



// Initialize the page
initializePage();


function showRegistrations() {
    window.open("http://localhost:5146/users", "_blank", "width=400,height=1200,left=1000,top=100");
}
