# WERM 0.1.0 Design for Security

**Content type:** Design for security (DFS)

**Project and release:** Waughtal Enterprise Resource Management 0.1.0

**Status:** Reviewed for engineering candidate; release residual risks remain
open

**Security owner:** Security owner

**Baseline:** WSP 1.1.0 Security/DFS profile

## Scope and Security Objectives

This design covers the WERM desktop application, its per-user configuration,
SQLite database access through the packaged ODBC driver, maintenance
authorization, audit history, Word automation and templates, Waughtal Shell
database installation, WPM installation, and release production.

The security objectives are to:

- prevent an unauthenticated WERM GUI session from changing controlled data;
- avoid storing or logging the maintenance password;
- preserve the integrity and lineage of changes made through WERM;
- reject malformed, unrelated, partial, and newer database schemas without
  replacing pre-existing files;
- constrain release dependencies and artifacts to identifiable inputs; and
- fail without printing an incomplete label or leaving a partially-created
  database after a failed first migration.

WERM 0.1.0 does not encrypt the SQLite database, isolate mutually hostile
Windows users, protect against a local administrator, sandbox Microsoft Word,
provide a network service, or establish multi-user database authorization.

## Assets and Protection Needs

| Asset | Confidentiality | Integrity | Availability |
| --- | --- | --- | --- |
| Customer-specific prices | Business-sensitive | A wrong price can produce a materially incorrect label | Needed for customer labels |
| Product description, ingredients, and safe-handling status | Normal business data | Safety and labeling correctness require accurate data | Needed for labels |
| Maintenance verifier and transient password | The plaintext password must never persist; the verifier must be protected from offline attack | Unauthorized replacement would defeat GUI authorization | Needed for maintenance |
| Product and price audit history | May identify operators and business changes | Must remain append-only through supported interfaces | Needed for accountability |
| SQLite database and backups | Contains the assets above | Direct file writes can bypass WERM controls | Needed for operation and recovery |
| Per-user settings | Contains paths and ODBC identifiers but no credentials | Tampering can redirect WERM to another database, driver, or template | Needed for reliable startup |
| Word template and printer selection | Template content may be proprietary | A malicious or incorrect template can alter output or execute Word behavior | Needed for correct physical labels |
| WERM binaries, ODBC driver, WSH, WPM package, and build evidence | Public after release | Substitution can introduce arbitrary code | Needed to install and support WERM |
| Signing identities | Secret/private | Misuse could create trusted malicious artifacts | Needed for a trusted public release |

## Trust Model and Boundaries

| Boundary | Trusted side | Untrusted or less-trusted side | Enforcement |
| --- | --- | --- | --- |
| Operator to WERM GUI | Application authorization services | Keyboard input and an unattended interactive session | Password verification, short session, logout/expiry, validation |
| WERM to SQLite file | Parameterized repository and controlled schema | Configured path, file contents, and direct external editors | Schema validation, transactions, triggers, Windows ACLs |
| WERM to ODBC | Pinned packaged driver after controlled installation | Driver/DSN names and machine registrations | Architecture-specific registry inventory, explicit driver identity, deployment verification |
| WERM to per-user settings | Versioned settings contract | XML file and environment overrides | DTD prohibited, resolver disabled, value validation, atomic replacement, visible overrides |
| WERM to Word | Required tagged-control contract | Template contents, Word installation, add-ins, macros, printer driver, and spooler | Exact tag validation, approved template procedure, Word policy, physical release test |
| Installer to Windows | Reviewed package scripts | Administrative filesystem, registry, and environment state | Explicit versioned paths, architecture checks, clean install/removal test |
| Source to release artifact | Pinned repository revision and controlled workflow | Downloaded dependencies and build runner | Exact revisions, SHA-256 checks, provenance records, signing and Defender gates |

The normal WERM process runs as the interactive user. Administrative rights
are required only for WPM installation/removal and machine-wide ODBC driver
registration. WERM does not treat a local administrator, a process running as
the database-file owner, Word itself, or a configured third-party ODBC driver
as a hostile process it can contain.

## Security-Relevant Data Flows

1. The operator selects product/customer/price data. WERM issues parameterized
   ODBC reads against a validated schema and maps the result to nine controlled
   Word fields.
2. The operator enters the maintenance passphrase. WERM derives a verifier in
   memory, compares it in fixed time, and returns only a non-transferable
   in-process session token. The plaintext is not written to settings, logs,
   command lines, or the database.
3. An authorized maintenance operation validates the session and input, then
   changes the primary record and writes audit lineage within one transaction.
4. WERM loads `%LOCALAPPDATA%\WERM\settings.xml`; managed `WERM_*`
   environment overrides take precedence and are shown in the configuration
   screen. Settings contain no credentials.
5. WERM asks Word to create a working document from the selected template,
   validates every required tag, writes values, invokes direct printing, and
   closes the unsaved working document even after an error.
6. WPM installs versioned binaries below Program Files and registers the exact
   architecture's ODBC driver. Release production signs the finalized PE
   files, packages them, signs the WPM envelope, scans the exact bytes, and
   records digests before publication.

## Threat Analysis and Controls

| ID | Threat and consequence | Existing or required control | Verification or evidence |
| --- | --- | --- | --- |
| T-01 | Password guessing enables unauthorized maintenance | 15–256 character passphrases, PBKDF2-HMAC-SHA512 at 220,000 iterations, random 32-byte salt, 64-byte verifier, five-failure/30-second process lockout | `TC-0009`, `TC-0011`, ADR-0010 |
| T-02 | Password or verifier disclosure | Password never persists or appears in settings; fixed-time comparison; credential row stores algorithm/work factor/salt/hash only | `TC-0018`, `TC-0029`, source review |
| T-03 | Reuse of an unattended authorized session | Fixed ten-minute, in-process, non-transferable session; logout, expiry, and password change revoke authorization | `TC-0010`, `TC-0016` |
| T-04 | Direct SQLite editing bypasses GUI authorization or audit | Database directory and backups require OS access control; documentation states the boundary; no release claim says the password encrypts or protects direct access | ADR-0005, ADR-0010, operating review |
| T-05 | A failed write changes data without audit lineage | Primary and audit changes share one transaction; append-only triggers reject supported update/delete attempts | `TC-0013`–`TC-0017` |
| T-06 | Malicious paths, settings XML, or database contents cause unsafe parsing or replacement | Canonical paths, versioned XML, DTD prohibition, null resolver, input/schema validation, atomic settings writes, transactional migration, preserve pre-existing files | `TC-0007`, `TC-0008`, `TC-0029`, `TC-0030` |
| T-07 | SQL input alters statement structure | Repository commands use positional ODBC parameters; schema identifiers and migration text are controlled source | Repository review and controlled tests |
| T-08 | Wrong-bitness or substituted ODBC/WSH dependency executes unreviewed code | Exact upstream revision/archive hashes, source build, dependency manifest, architecture checks, machine registration verification | `TC-0025`, dependency manifest, CI evidence |
| T-09 | A malicious or macro-enabled Word template executes behavior or changes label meaning | Only reviewed templates in access-controlled locations; organizational Word macro/add-in policy; exact nine-tag contract; hash and environment recorded by physical test | `TC-0021`, `TC-0022`, `TC-0027`; physical gate remains open |
| T-10 | Missing fields or Word failure prints a partial label or leaks a Word process | Validate all required tags before printing; unsaved working-document cleanup in `finally`; physical cleanup verification | `TC-0022`, `TC-0024`, `TC-0027` |
| T-11 | Package installation overwrites unrelated state or removal destroys data | Versioned install root, external SQLite/settings excluded from removal, explicit registry actions, disposable-runner clean install/remove test | `TC-0028` |
| T-12 | Build or release artifact substitution compromises workstations | Pinned actions/tools/dependencies, SHA-256 verification, native version resources, exact-source CI, protected Authenticode identity, timestamp, Defender scan, WPM signature | CI plus Windows release trust record; signing gate remains open |
| T-13 | Logs or evidence disclose operational data/secrets | CI uses synthetic databases; tools must not print secrets; settings/credential tests inspect persistence; signing keys never enter repository or ordinary logs | `TC-0018`, `WSP-TOOL-0007`, review |
| T-14 | Loss or corruption of the single SQLite file prevents operation | Operator-controlled backups while WERM is closed, transaction rollback, preservation of pre-existing files, documented recovery boundary | Database installation procedure and failure tests |

## Derived Security Requirements

| ID | Requirement | Allocation and status |
| --- | --- | --- |
| DFS-SR-001 | Every GUI write shall demand a valid maintenance session immediately before invoking the write service. | Implemented; `REQ-0012`, `TC-0016` |
| DFS-SR-002 | WERM shall never persist or log the plaintext maintenance password. | Implemented; ADR-0010, `TC-0018`, `TC-0029` |
| DFS-SR-003 | Supported data changes and their audit lineage shall commit or roll back atomically. | Implemented; `REQ-0013`–`REQ-0015`, `TC-0013`–`TC-0017` |
| DFS-SR-004 | Creation or migration shall not replace a pre-existing unrecognized, partial, or newer database. | Implemented; `REQ-0020`, `REQ-0022`, `TC-0008`, `TC-0025`, `TC-0030` |
| DFS-SR-005 | Release operation shall restrict database and approved-template write access to authorized Windows principals and protect backups equivalently. | Operational release gate; open until workstation review |
| DFS-SR-006 | A physical-print release test shall identify the exact template digest, Word version, printer/driver, and stock and shall use the production macro/add-in policy. | Open; `TC-0027` |
| DFS-SR-007 | Every distributed PE shall have verified release identity, protected SHA-256 Authenticode signature, trusted timestamp, and clean exact-artifact Defender evidence before publication. | Version identity implemented; signing/scan open |
| DFS-SR-008 | The final WPM envelope shall be signed only after its PE payload is finalized and independently verified. | Open release gate |
| DFS-SR-009 | Security reports shall use a private intake, controlled triage/remediation, verified update, and coordinated disclosure process. | Defined in `SECURITY.md` |

## Security Verification and Review

Automated authorization, credential persistence, transaction, input, failure,
settings, migration, ODBC, package-install, and version-resource tests run on
both x86 and x64 GitHub runners. CI evidence identifies the source revision and
architecture. The security reviewer must additionally inspect dependency
provenance, the exact release packages, the signing record, Defender results,
the database/template ACLs, and the `TC-0026`/`TC-0027` physical evidence.

A security-relevant failure blocks an unqualified release. Findings are
corrected, explicitly accepted by the release/security owner with scope and
review condition, or carried as a release-blocking open item. A normal test
pass cannot waive direct-file, Word/template, signing, or physical-printer
risks outside that test's boundary.

## Failure, Recovery, and Vulnerability Response

On failed first-time migration WERM rolls back and removes only the new
incomplete file it created. It never replaces a pre-existing database.
Operators back up the database while it is not being modified and protect the
backup with equivalent Windows access controls. A suspected compromised
binary, signing identity, dependency, template, or database is removed from
release consideration until its origin and impact are assessed.

Private vulnerability intake, affected-version analysis, remediation,
verification, disclosure, and supported-update distribution are defined in
the repository [security policy](../SECURITY.md). Signing-identity compromise
also triggers certificate revocation/replacement and withdrawal of affected
packages.

## Residual Risks and Release Conditions

| Residual risk | Present disposition |
| --- | --- |
| A Windows principal with direct SQLite write access can bypass WERM authorization and audit | Accepted design boundary only when release workstation/database ACL review passes; otherwise release-blocking |
| Process restart clears the in-memory guessing lockout | Documented limitation mitigated by passphrase length/work factor and workstation controls |
| Word templates, macros, add-ins, printer drivers, and Word itself execute outside WERM's containment | Release-blocking until approved-template/Word-policy and physical evidence pass |
| Customer/product data are not encrypted at rest by WERM | Requires OS access control and protected backups; must be accepted for the deployment |
| Final Authenticode, WPM signing, timestamp, and Defender evidence do not yet exist | Release-blocking; engineering packages are not publishable releases |
| Local single-file SQLite availability depends on operator backup and workstation health | Operational risk documented for 0.1.0 |

## References

- [ADR-0010: Hash the maintenance password](adr-0010-hash-maintenance-password.md)
- [ADR-0011: ODBC repository transactions](adr-0011-use-odbc-repository-transactions.md)
- [ADR-0013: Per-user database configuration](adr-0013-use-per-user-database-configuration.md)
- [Database design](database-design.md)
- [Database installation](database-installation.md)
- [Word template contract](word-template-contract.md)
- [WERM security policy](../SECURITY.md)
- [WSP Security/DFS requirements](../wsp/security/security-requirements.md)
