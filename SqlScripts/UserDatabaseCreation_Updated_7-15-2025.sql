--CREATE DATABASE ForagerDB

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
    UssUsrId UNIQUEIDENTIFIER,
	CONSTRAINT FK_UserSecurity_Users FOREIGN KEY (UssUsrId) REFERENCES Users(UsrId),
    UssUsername NVARCHAR(20),
    UssPassword NVARCHAR(20),
    UssLastLoginDate DATETIME null,
    UssLastLogoffDate DATETIME null    
)

CREATE TABLE UserMessages (
    UsmId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),

    UsmUsrId UNIQUEIDENTIFIER NOT NULL, -- optional, might be redundant with sender/recipient
    UsmSubject NVARCHAR(50),
    UsmMessage NVARCHAR(MAX),
    UsmSendDate DATETIME NOT NULL,
    UsmReceivedDate DATETIME NULL,

    UsmSenderId UNIQUEIDENTIFIER NOT NULL,
    UsmRecipientId UNIQUEIDENTIFIER NOT NULL,
    UsmStatus NVARCHAR(20) NOT NULL DEFAULT 'unread',

    CONSTRAINT FK_UserMessages_User FOREIGN KEY (UsmUsrId) REFERENCES Users(UsrId),
    CONSTRAINT FK_UserMessages_Sender FOREIGN KEY (UsmSenderId) REFERENCES Users(UsrId),
    CONSTRAINT FK_UserMessages_Recipient FOREIGN KEY (UsmRecipientId) REFERENCES Users(UsrId)
);



CREATE TABLE UserFinds (
    UsfId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
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
    UslUsfId UNIQUEIDENTIFIER,
    UslLatitude FLOAT,
    UslLongitude FLOAT,
    CONSTRAINT FK_UserFindLocation_UserFinds FOREIGN KEY (UslUsfId) REFERENCES UserFinds(UsfId)
);
 

CREATE TABLE UserImages (
    UsiId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UsiUsrId UNIQUEIDENTIFIER null,
    UsiUsfId UNIQUEIDENTIFIER null,
    UsiImageData NVARCHAR(MAX),
    CONSTRAINT FK_UserImages_Users FOREIGN KEY (UsiUsrId) REFERENCES Users(UsrId),
    CONSTRAINT FK_UserImages_UserFinds FOREIGN KEY (UsiUsfId) REFERENCES UserFinds(UsfId)
);


CREATE TABLE UserFindsComments (
    UscId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UscComment NVARCHAR(MAX),
    UscCommentScore INT NULL,
    UscCommentDate DATETIME,
    UscParentCommentId UNIQUEIDENTIFIER NULL,
    CONSTRAINT FK_UserFindsComment_ParentComment FOREIGN KEY (UscParentCommentId) REFERENCES UserFindsComments(UscId)                
);


Create table UserFindsCommentXref (
UcxId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
UcxUsrId UNIQUEIDENTIFIER,
UcxUscId UNIQUEIDENTIFIER,
UcxUsfId UNIQUEIDENTIFIER,
CONSTRAINT FK_UserFindsCommentXref_Users FOREIGN KEY (UcxUsrId) REFERENCES Users(UsrId),       
CONSTRAINT FK_UserFindsCommentXref_UserFindsComments FOREIGN KEY (UcxUscId) REFERENCES UserFindsComments(UscId),
CONSTRAINT FK_UserFindsCommentXref_UserFinds FOREIGN KEY (UcxUsfId) REFERENCES UserFinds(UsfId) 
)









