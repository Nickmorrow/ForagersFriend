CREATE TABLE UserLocation (
UslId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
UslLatitude FLOAT,
UslLongitude FLOAT
)

CREATE TABLE UserImages (
    UsiId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UsiImageData VARBINARY(MAX)
)

create table UserSecurity (
UssId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
UssUsername NVARCHAR(20),
UssPassword NVARCHAR(20),
UssLastLoginDate Datetime,
UssLastLogoffDate Datetime
)


create table UserMessages (
UsmId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
UsmSubject Nvarchar(50),
UsmMessage Nvarchar(max),
UsmSendDate Datetime,
UsmReceivedDate Datetime
)

Create table Users (
UsrId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
UsrUssId UNIQUEIDENTIFIER,
UsrUsiId UNIQUEIDENTIFIER null,
UsrUslId UNIQUEIDENTIFIER null,
UsrUsmId UNIQUEIDENTIFIER null,
CONSTRAINT FK_Users_UserSecurity FOREIGN KEY (UsrUssId)
        REFERENCES UserSecurity(UssId),
CONSTRAINT FK_Users_UserImages FOREIGN KEY (UsrUsiId)
        REFERENCES UserImages(UsiId),
CONSTRAINT FK_Users_UserLocation FOREIGN KEY (UsrUslId)
        REFERENCES UserLocation(UslId),
CONSTRAINT FK_Users_UserMessages FOREIGN KEY (UsrUsmId)
        REFERENCES UserMessages(UsmId),
UsrName NVARCHAR(50) null,
UsrEmail NVARCHAR(50),
UsrFindsNum int null,
UsrExpScore int null,
UsrJoinedDate DateTime,
UsrCountry NVARCHAR(50) null,
UsrStateorProvince NVARCHAR(50) null,
UsrZipCode int null

)

create table UserMessagesXref (
UmxId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
UmxUsrId UNIQUEIDENTIFIER,
UmxUsmId UNIQUEIDENTIFIER,
CONSTRAINT FK_UserMessagesXref_Users FOREIGN KEY (UmxUsrId)
        REFERENCES Users(UsrId),
CONSTRAINT FK_UserMessagesXref_UserMessages FOREIGN KEY (UmxUsmId)
        REFERENCES UserMessages(UsmId)
)

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
CONSTRAINT FK_UserFindsCommentXref_Users FOREIGN KEY (UcxUsrId)
        REFERENCES Users(UsrId),
CONSTRAINT FK_UserFindsCommentXref_UserFindsComments FOREIGN KEY (UcxUscId)
        REFERENCES UserFindsComments(UscId)
)


CREATE TABLE UserFinds (
    UsFId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UsfName NVARCHAR(50),
    UsfUsrId UNIQUEIDENTIFIER,
    UsfUslId UNIQUEIDENTIFIER,   
	UsfUsiId UNIQUEIDENTIFIER null,
	UsfUscId UNIQUEIDENTIFIER null,
    CONSTRAINT FK_UserFinds_Users FOREIGN KEY (UsfUsrId)
        REFERENCES Users(UsrId),
    CONSTRAINT FK_UserFinds_UserLocation FOREIGN KEY (UsfUslId)
        REFERENCES UserLocation(UslId),
	CONSTRAINT FK_UserFinds_UserImages FOREIGN KEY (UsfUsiId)
        REFERENCES UserImages(UsiId),
	CONSTRAINT FK_UserFinds_UserFindsComments FOREIGN KEY (UsfUscId)
        REFERENCES UserFindsComments(UscId),
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






