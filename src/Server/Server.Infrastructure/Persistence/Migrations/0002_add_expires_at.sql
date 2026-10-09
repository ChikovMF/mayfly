ALTER TABLE shared_files ADD COLUMN expires_at timestamptz NULL;

CREATE INDEX ix_shared_files_expires_at ON shared_files (expires_at) WHERE expires_at IS NOT NULL;
