ALTER TABLE [dbo].[groupReadActivity]
ADD CONSTRAINT [FK_groupReadActivity_user]
FOREIGN KEY ([userId])
REFERENCES [dbo].[user] ([id]);

ALTER TABLE [dbo].[groupReadActivity]
ADD CONSTRAINT [FK_groupReadActivity_establishment]
FOREIGN KEY ([establishmentId])
REFERENCES [dbo].[establishment] ([id]);
