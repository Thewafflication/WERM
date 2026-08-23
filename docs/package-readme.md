# WERM Installed Package

WERM 0.1.0 is a Windows label-printing application. It reads and maintains
SQLite data through ODBC, populates tagged Microsoft Word content controls, and
prints directly through Word. It does not generate a PDF or store/generate/
print a barcode.

Before launching WERM, read:

- `docs/workstation-configuration.md` for .NET,
  Word, SQLite ODBC, printer, bitness, and environment settings;
- `docs/database-installation.md` for the Waughtal Shell
  entry point and recovery behavior; and
- `docs/word-template-contract.md` for the nine exact
  required content-control tags.

Run `Werm.exe` after configuring the database connection. Printing remains
available while maintenance is locked. Product, customer, and price Save
buttons require an initialized password and authenticated operator session.
Product audit history is read-only in the Product area.

The package identity and source revision are in `.wpm/package.txt`. A CI or
engineering-candidate version is not an approved release unless accompanied by
the project's passing readiness and release records.
