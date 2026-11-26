# Cloud build notes

This repository is being prepared for a phone-managed GitHub Actions workflow.

## Current pipeline

- Unity editor: 6000.3.25f1 (Unity 6.3 LTS)
- Build action: GameCI Unity Builder
- Target: iOS
- Output: an Xcode project, not a signed IPA
- Trigger: Actions > Unity iOS Xcode Export > Run workflow

## Required before the first successful run

1. A complete Unity project, including at least one enabled scene in Build Settings.
2. A valid Unity Personal license file stored as the repository Actions secret named `UNITY_LICENSE`.
3. The repository must have GitHub Actions enabled.

Never commit a Unity license file, Apple certificate, private key, or provisioning profile into the repository. Store credentials only as GitHub Actions secrets.

## IPA and signing

Unity's iOS build step exports an Xcode project. Creating a distributable IPA is a separate Xcode/signing step. This workflow intentionally does not sign the app or embed anyone's Apple certificate. Players can use their own signing credentials in a separate, compatible signing workflow.

## Phone-only limitation

GitHub Actions can run cloud builds, but a Unity Personal license still has to be activated and supplied to the build environment. This workflow cannot create or bypass a Unity license. If the license file cannot be obtained without a desktop Unity Hub activation, the build will stop at the license check.
