use ForagerDB

ALTER TABLE UserFindsComments
ADD UscParentCommentId UNIQUEIDENTIFIER NULL;

ALTER TABLE UserFindsComments
ADD CONSTRAINT FK_UserFindsComment_ParentComment
FOREIGN KEY (UscParentCommentId) REFERENCES UserFindsComments(UscId);