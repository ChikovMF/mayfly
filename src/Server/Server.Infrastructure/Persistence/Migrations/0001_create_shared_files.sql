CREATE TABLE shared_files (
    id           text   PRIMARY KEY,
    file_name    text   NOT NULL,
    content_type text   NOT NULL,
    size         bigint NOT NULL
);
