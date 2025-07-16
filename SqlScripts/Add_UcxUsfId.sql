ALTER TABLE UserFindsCommentXref
ADD UcxUsfId UNIQUEIDENTIFIER;

ALTER TABLE UserFindsCommentXref
ADD CONSTRAINT FK_UserFindsCommentXref_UserFinds
    FOREIGN KEY (UcxUsfId) REFERENCES UserFinds(UsfId);

