# Real-time translations

This document exists as a live test input for the automatic translation pipeline.

## What the service does

The service runs on a schedule and validates the translation server, configuration, and available languages before any translation work starts.

After the validation step, it synchronizes country names from the read-only countries catalog into the standard localization JSON dictionaries. If the application default language is English, the country entry is stored as key equals value. If the default language is different, the English country name is first translated into the default language, and only then stored as key equals value in the default dictionary.

Next, the service compares the current default localization dictionary with the stored snapshot from the previous run. Newly added entries are translated into target languages only when the key does not already exist, so manual translations keep priority. Removed entries are deleted from all target dictionaries to keep the whole set consistent.

Finally, the service scans configured documentation roots for Markdown trees. Each topic folder is expected to contain a source file named after the default language, such as en.md. The service hashes that source file, detects changes, translates missing or outdated target Markdown files, and stores the current hash next to the source file. If writing the hash next to the source file is not possible, it falls back to temporary storage.

## How the service reports progress

The backend emits general SignalR messages through the localization hub using one message envelope. Every message carries a message type, the current process stage, a UTC timestamp, a text summary, and optional stage-specific payload.

The current stages are:

- CheckServers
- TranslateCountries
- TranslateJsonFiles
- TranslateMarkdownFiles
- StoringResults

Typical message flow is stage started, stage completed, and pipeline completed. If a stage fails, the message is marked as an error and includes structured error information with unified error codes.

## Design principles

Translations are processed sequentially to avoid overloading the LibreTranslate server.

Localization JSON dictionaries are always stored with alphabetically sorted keys and formatted JSON for easier maintenance.

The previous default dictionary snapshot is stored persistently so a restart of the application does not lose change tracking.

Manual translations always have priority over automatic additions.