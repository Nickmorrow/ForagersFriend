use ForagerDB

CREATE TABLE UserFindsCommentLikes (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    UserId UNIQUEIDENTIFIER NOT NULL,
    UscId UNIQUEIDENTIFIER NOT NULL,
    IsLike BIT NOT NULL,
    CONSTRAINT FK_UserFindsCommentLikes_UserFindsComment FOREIGN KEY (UscId) REFERENCES UserFindsComments(UscId),
    CONSTRAINT FK_UserFindsCommentLikes_User FOREIGN KEY (UserId) REFERENCES Users(UsrId))