ALTER TABLE shared_files ALTER COLUMN sha256 DROP NOT NULL;

ALTER TABLE shared_files
    ADD CONSTRAINT ck_shared_files_completed_has_sha256 CHECK ((completed_at IS NULL) = (sha256 IS NULL));
