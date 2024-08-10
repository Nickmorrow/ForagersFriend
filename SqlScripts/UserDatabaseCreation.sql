use ForagerDB

Create table Users (
UsrId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
UsrName NVARCHAR(50) null,
UsrBio NVARCHAR(1000),
UsrEmail NVARCHAR(50),
UsrFindsNum int null,
UsrExpScore int null,
UsrJoinedDate DateTime,
UsrCountry NVARCHAR(50) null,
UsrStateorProvince NVARCHAR(50) null,
UsrZipCode int null

)

CREATE TABLE UserSecurity (
    UssId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UsrId UNIQUEIDENTIFIER,
	CONSTRAINT FK_UserSecurity_Users FOREIGN KEY (UsrId) REFERENCES Users(UsrId),
    UssUsername NVARCHAR(20),
    UssPassword NVARCHAR(20),
    UssLastLoginDate DATETIME null,
    UssLastLogoffDate DATETIME null    
)

CREATE TABLE UserMessages (
    UsmId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UsrId UNIQUEIDENTIFIER,
    UsmSubject NVARCHAR(50),
    UsmMessage NVARCHAR(MAX),
    UsmSendDate DATETIME,
    UsmReceivedDate DATETIME null,
    CONSTRAINT FK_UserMessages_Users FOREIGN KEY (UsrId) REFERENCES Users(UsrId)
);


create table UserMessagesXref (
UmxId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
UmxUsrId UNIQUEIDENTIFIER,
UmxUsmId UNIQUEIDENTIFIER,
CONSTRAINT FK_UserMessagesXref_Users FOREIGN KEY (UmxUsrId) REFERENCES Users(UsrId),
CONSTRAINT FK_UserMessagesXref_UserMessages FOREIGN KEY (UmxUsmId) REFERENCES UserMessages(UsmId)       
)


CREATE TABLE UserFinds (
    UsFId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UsfName NVARCHAR(50),
    UsfUsrId UNIQUEIDENTIFIER,
    CONSTRAINT FK_UserFinds_Users FOREIGN KEY (UsfUsrId) REFERENCES Users(UsrId),       
	UsfFindDate Datetime,
	UsfSpeciesName Nvarchar(50),
	UsfSpeciesType Nvarchar(50),
	UsfUseCategory Nvarchar(50) null,
	UsfFeatures Nvarchar(Max) null,
	UsfLookAlikes Nvarchar(Max) null,
	UsfHarvestMethod Nvarchar(Max) null,
	UsfTastesLike Nvarchar(Max) null,
	UsfDescription Nvarchar(Max) null,
	UsfAccuracyScore int null
)


CREATE TABLE UserFindLocation (
    UslId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UsfId UNIQUEIDENTIFIER,
    UslLatitude FLOAT,
    UslLongitude FLOAT,
    CONSTRAINT FK_UserFindLocation_UserFinds FOREIGN KEY (UsfId) REFERENCES UserFinds(UsfId)
);
 

CREATE TABLE UserImages (
    UsiId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UsrId UNIQUEIDENTIFIER null,
    UsfId UNIQUEIDENTIFIER null,
    UsiImageData VARBINARY(MAX),
    CONSTRAINT FK_UserImages_Users FOREIGN KEY (UsrId) REFERENCES Users(UsrId),
    CONSTRAINT FK_UserImages_UserFinds FOREIGN KEY (UsfId) REFERENCES UserFinds(UsfId)
);


Create table UserFindsComments (
UscId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
UscComment Nvarchar(Max),
UscCommentScore int null,
UscCommentDate Datetime
)


Create table UserFindsCommentXref (
UcxId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
UcxUsrId UNIQUEIDENTIFIER,
UcxUscId UNIQUEIDENTIFIER,
CONSTRAINT FK_UserFindsCommentXref_Users FOREIGN KEY (UcxUsrId) REFERENCES Users(UsrId),       
CONSTRAINT FK_UserFindsCommentXref_UserFindsComments FOREIGN KEY (UcxUscId) REFERENCES UserFindsComments(UscId)       
)









