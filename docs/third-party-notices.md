# Third-Party Notices

WERM 0.1.0 packages include the architecture-matched SQLite ODBC runtime
described below. These notices do not change WERM's own license.

## SQLite ODBC Driver 0.99991

- Upstream: Christian Werner's SQLite ODBC Driver
- Reproducible source mirror revision:
  `539531394dcedf415de574daa95367a93f5eb41d`
- Source archive SHA-256:
  `13f29b7ed20ee2408c2e0812a0ca43e33021a6548ede99b95e04e414718b3c0a`
- License: BSD-style terms reproduced as `sqliteodbc-license.terms` in each
  WPM package

The WERM build applies one pointer-width portability cast for current MSVC and
removes four unsupported legacy linker directives. The ODBC export list and
runtime behavior remain upstream. The driver uses the static Microsoft C
runtime so it introduces no separate Visual C++ redistributable prerequisite.

## SQLite 3.43.2

- Upstream: SQLite amalgamation 3.43.2
- Source archive SHA-256:
  `a17ac8792f57266847d57651c5259001d1e4e4b46be96ec0d985c953925b2a1c`
- Project statement: SQLite source code is dedicated to the public domain

The exact dependency revisions, input hashes, target architecture, and built
driver hash are recorded in each package's `dependency-manifest.json`.
