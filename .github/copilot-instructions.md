# Copilot Instructions

## Project Guidelines
- For this ForagersFriend project, Azure Blob Storage is no longer used; implement media storage against the local filesystem unless the user says otherwise.
- ForagersFriend currently has no email verification system. Existing profile email values are unverified and are made-up/test addresses; the app only performs basic email-format validation. Do not treat imported profile emails as verified recovery addresses during the Identity migration.
- Email confirmation should be disabled only in Development so test accounts can use fake addresses; Production must continue requiring verified email.
- On registration throttling, show the cooldown validation on the registration form and disable the register button until the retry window expires instead of showing a separate HTTP 429 page.
- On registration validation failures, preserve non-sensitive form fields instead of clearing them; do not persist or restore password fields.
- For registration, provide validation feedback specific to the failing field or rule instead of a generic failure message.
- For ForagersFriend, Docker is the intended deployment method; ensure HTTPS support for the Docker deployment.