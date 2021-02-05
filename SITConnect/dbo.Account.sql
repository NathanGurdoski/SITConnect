CREATE TABLE [dbo].[Account] (
    [Id]              INT            IDENTITY (1, 1) NOT NULL,
    [Fname]           NCHAR (20)     NULL,
    [Lname]           NCHAR (20)     NULL,
    [Email]           NCHAR (30)     NULL,
    [PasswordHash]    NVARCHAR (MAX) NULL,
    [PasswordSalt]    NVARCHAR (MAX) NULL,
    [CreditNo]        NVARCHAR (MAX) NULL,
    [CVC]             NVARCHAR (MAX) NULL,
    [ExpDate]         NVARCHAR (MAX) NULL,
    [DateTimeLockout] NVARCHAR (50)  NULL,
    [IV]              NVARCHAR (MAX) NULL,
    [Key]             NVARCHAR (MAX) NULL,
    [PwdSetDateTime]  NVARCHAR (50)  NULL,
    [FirstGenPwdHash] NVARCHAR (MAX) NULL,
    [FirstGenPwdSalt] NVARCHAR (MAX) NULL,
    [SecGenPwdHash]   NVARCHAR (MAX) NULL,
    [SecGenPwdSalt]   NVARCHAR (MAX) NULL,
    [LastLogin]       NVARCHAR (50)  NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

