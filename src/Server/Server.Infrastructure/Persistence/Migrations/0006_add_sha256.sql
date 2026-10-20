DELETE FROM shared_files;
ALTER TABLE shared_files ADD COLUMN sha256 bytea NOT NULL;
