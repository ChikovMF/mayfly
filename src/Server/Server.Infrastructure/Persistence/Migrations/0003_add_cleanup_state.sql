ALTER TABLE shared_files
    ADD COLUMN cleanup_after    timestamptz NULL,
    ADD COLUMN cleanup_attempts integer     NOT NULL DEFAULT 0;
