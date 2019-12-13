USE [Ninjacators]
GO

/****** Object:  Table [dbo].[AccessToken]    Script Date: 12/13/2019 12:13:34 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[AccessToken](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[client_id] [nvarchar](50) NOT NULL,
	[access_token] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_AccessToken] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

USE [Ninjacators]
GO

/****** Object:  Table [dbo].[EventKey]    Script Date: 12/13/2019 12:13:51 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE TABLE [dbo].[EventKey](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[client_id] [nvarchar](50) NOT NULL,
	[event_key] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_EventKey] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

USE [Ninjacators]
GO

/****** Object:  Table [dbo].[HookSecret]    Script Date: 12/13/2019 12:14:55 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE TABLE [dbo].[HookSecret](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[client_id] [nvarchar](50) NOT NULL,
	[x_hook_secret] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_HookSecret] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO


USE [Ninjacators]
GO

/****** Object:  Table [dbo].[ReferringAction]    Script Date: 12/13/2019 12:15:26 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[ReferringAction](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[client_id] [nvarchar](50) NOT NULL,
	[referring_action] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_ReferringAction] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO