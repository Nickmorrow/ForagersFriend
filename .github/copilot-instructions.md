# Copilot Instructions

## Project Guidelines
- For this ForagersFriend project, Azure Blob Storage is no longer used; implement media storage against the local filesystem unless the user says otherwise.
- ForagersFriend currently has no email verification system. Existing profile email values are unverified and are made-up/test addresses; the app only performs basic email-format validation. Do not treat imported profile emails as verified recovery addresses during the Identity migration. Production must implement a working email verification service; imported development addresses must not be treated as verified recovery addresses.