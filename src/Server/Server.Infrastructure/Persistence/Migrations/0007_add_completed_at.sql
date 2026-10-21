ALTER TABLE shared_files ADD COLUMN completed_at timestamptz NULL;

UPDATE shared_files SET completed_at = now();

CREATE INDEX ix_shared_files_unfinished ON shared_files (cleanup_after) WHERE completed_at IS NULL;
