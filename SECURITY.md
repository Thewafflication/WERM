# WERM Security Policy

## Supported Versions

WERM has not yet published a supported release. Security corrections made
before 0.1.0 are applied to `master`. After 0.1.0 is approved and published,
the latest 0.1.x release will receive security corrections until a later
support policy supersedes this one.

Engineering candidates, unsigned packages, and untagged source revisions are
not supported releases.

## Reporting a Vulnerability

Do not disclose a suspected vulnerability in a public issue. Use the
repository's private **Report a vulnerability** form:

<https://github.com/Thewafflication/WERM/security/advisories/new>

Include the affected version or source revision, operating environment,
reproduction steps, impact, and any proposed mitigation. Do not include live
passwords, customer data, private templates, signing material, or database
files containing operational data.

The project owner will acknowledge the report, assess severity and affected
versions, preserve it as a controlled security finding, and coordinate a fix,
verification, disclosure, and release decision. Timing depends on severity
and reproducibility; the reporter will be updated when the assessment or
planned resolution materially changes.

## Security Boundary

The maintenance password authorizes changes made through WERM. It does not
encrypt the SQLite file and cannot prevent a Windows user or administrator
with direct write access from changing that file outside WERM. Protect the
database directory, backups, approved Word templates, and workstation with
Windows access control and normal organizational security controls.

Only packages named in an approved release record, carrying the documented
publisher signature and matching published SHA-256 digests, are release
artifacts. A future security update will be distributed through the same WPM
and GitHub release process as the supported release it replaces.
