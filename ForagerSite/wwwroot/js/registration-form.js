const storageKey = "registration-form-draft";
const preservedFields = ["Name", "Email", "Country", "StateOrProvince", "Username"];
const cooldownTimers = new WeakMap();
const submitHandlers = new WeakMap();

export function initialize(form, clearDraft, rateLimitUntil) {
    try {
        if (clearDraft) {
            sessionStorage.removeItem(storageKey);
        } else {
            const draft = JSON.parse(sessionStorage.getItem(storageKey) ?? "{}");
            for (const name of preservedFields) {
                const input = form.elements.namedItem(name);
                if (input && typeof draft[name] === "string") {
                    input.value = draft[name];
                }
            }
            sessionStorage.removeItem(storageKey);
        }
    } catch {
        try {
            sessionStorage.removeItem(storageKey);
        } catch {
        }
    }

    const submitButton = form.querySelector('button[type="submit"]');
    const rateLimitMessage = document.getElementById("registration-rate-limit-message");
    let cooldownTimer;
    const updateCooldown = () => {
        const remainingSeconds = Math.max(0, Math.ceil(Number(rateLimitUntil) - Date.now() / 1000));
        if (submitButton) {
            submitButton.disabled = remainingSeconds > 0;
        }
        if (rateLimitMessage) {
            rateLimitMessage.hidden = remainingSeconds === 0;
            if (remainingSeconds > 0) {
                const minutes = Math.floor(remainingSeconds / 60);
                const seconds = remainingSeconds % 60;
                const remaining = minutes > 0 ? `${minutes}m ${seconds}s` : `${seconds}s`;
                rateLimitMessage.textContent = `Too many registration attempts. Try again in ${remaining}.`;
            }
        }
        if (remainingSeconds === 0 && cooldownTimer) {
            window.clearInterval(cooldownTimer);
            cooldownTimers.delete(form);
        }
    };

    if (Number(rateLimitUntil) > 0) {
        updateCooldown();
        if (Number(rateLimitUntil) > Date.now() / 1000) {
            cooldownTimer = window.setInterval(updateCooldown, 1000);
            cooldownTimers.set(form, cooldownTimer);
        }
    }

    const submitHandler = event => {
        if (Number(rateLimitUntil) > Date.now() / 1000) {
            event.preventDefault();
            updateCooldown();
            return;
        }

        try {
            const draft = Object.fromEntries(preservedFields.map(name => [
                name,
                form.elements.namedItem(name)?.value ?? ""
            ]));
            sessionStorage.setItem(storageKey, JSON.stringify(draft));
        } catch {
        }
    };
    form.addEventListener("submit", submitHandler);
    submitHandlers.set(form, submitHandler);
}

export function dispose(form) {
    const timer = cooldownTimers.get(form);
    if (timer) {
        window.clearInterval(timer);
        cooldownTimers.delete(form);
    }

    const submitHandler = submitHandlers.get(form);
    if (submitHandler) {
        form.removeEventListener("submit", submitHandler);
        submitHandlers.delete(form);
    }
}
