ALTER TABLE shared_files ADD COLUMN downloads_left integer NULL;
ALTER TABLE shared_files ADD COLUMN downloads_exhausted_at timestamptz NULL;

CREATE INDEX ix_shared_files_downloads_exhausted_at ON shared_files (downloads_exhausted_at) WHERE downloads_exhausted_at IS NOT NULL;
