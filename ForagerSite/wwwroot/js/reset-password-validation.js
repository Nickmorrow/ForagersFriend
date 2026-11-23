const form = document.getElementById("resetPasswordForm");

if (form) {
    const password = form.elements.namedItem("Password");
    const confirmation = form.elements.namedItem("ConfirmPassword");
    const passwordError = document.getElementById("password-error");
    const confirmationError = document.getElementById("confirm-password-error");

    const validateConfirmation = () => {
        const mismatch = confirmation.value.length > 0 && confirmation.value !== password.value;
        const message = mismatch ? "The passwords do not match." : "";
        confirmation.setCustomValidity(message);
        confirmationError.textContent = message;
    };

    const validatePassword = () => {
        const errors = [];
        if (password.value.length < 8)
            errors.push("Use at least 8 characters.");
        if (!/\p{Lu}/u.test(password.value))
            errors.push("Add an uppercase letter.");
        if (!/\p{Ll}/u.test(password.value))
            errors.push("Add a lowercase letter.");
        if (!/\p{Nd}/u.test(password.value))
            errors.push("Add a number.");
        if (!/[^\p{L}\p{N}]/u.test(password.value))
            errors.push("Add a symbol.");

        const message = errors.join(" ");
        password.setCustomValidity(message);
        passwordError.textContent = message;
        validateConfirmation();
    };

    password.addEventListener("input", validatePassword);
    confirmation.addEventListener("input", validateConfirmation);
    form.addEventListener("submit", () => {
        validatePassword();
        validateConfirmation();
    });
}
