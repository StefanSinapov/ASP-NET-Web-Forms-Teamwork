﻿namespace BarterSystem.WebForms.Account
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Web.UI;

    using BarterSystem.Common;
    using BarterSystem.Data;
    using BarterSystem.Models;
    using BarterSystem.WebForms.Controls.Notifier;

    using Microsoft.AspNet.Identity;

    public partial class ChangeAvatar : Page
    {
        private readonly BarterSystemData data = new BarterSystemData();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!this.IsPostBack)
            {
                var userId = User.Identity.GetUserId();
                var userAvatar = this.data.Users.All().Where(u => u.Id == userId).Select(u => u.AvatarUrl).First();
                this.Avatar.ImageUrl = "~/Imgs/" + userAvatar;
            }
        }

        protected void ButtonUpload_OnClick(object sender, EventArgs e)
        {
            if (this.FileUploadAvatar.HasFile)
            {
                try
                {
                    if (this.FileUploadAvatar.PostedFile.ContentLength > 1024000)
                    {
                        Notifier.Error("File has to be less than 1MB");
                    }
                    else
                    {
                        Stream fs = this.FileUploadAvatar.PostedFile.InputStream;
                        var br = new BinaryReader(fs);
                        var bytesPhoto = br.ReadBytes((int)fs.Length);
                        string base64String = Convert.ToBase64String(bytesPhoto, 0, bytesPhoto.Length);
                        this.Avatar.ImageUrl = "data:image/png;base64," + base64String;
                        string fileName = this.FileUploadAvatar.PostedFile.FileName;
                        var fileExtension = fileName.Substring(fileName.LastIndexOf('.'));
                        var newName = Guid.NewGuid() + fileExtension;
                        this.FileUploadAvatar.SaveAs(Server.MapPath(GlobalConstants.ImagesPath + newName));

                        this.SaveAvatarToUser(newName);
                        Notifier.Success("Image updated successfully");
                    }
                }
                catch (Exception exception)
                {
                    Notifier.Error(exception.Message);
                }
            }
            else
            {
                Notifier.Error("Please select file first");
            }
        }

        protected void ButtonUploadControl_OnClick(object sender, EventArgs e)
        {
            var imageName = Guid.NewGuid().ToString();
            var filePath = Server.MapPath(GlobalConstants.ImagesPath + imageName);
            string extension;

            try
            {
                extension = this.ControlImageFromUrl.DownloadRemoteImageFile(filePath);
            }
            catch (Exception error)
            {
                Notifier.Error(error.Message);
                return;
            }

            imageName = imageName + '.' + extension;

            this.Avatar.ImageUrl = GlobalConstants.ImagesPath + imageName;

            this.SaveAvatarToUser(imageName);
            Notifier.Success("Image updated successfully");
        }

        private void SaveAvatarToUser(string imageName)
        {
            var userId = this.User.Identity.GetUserId();
            var user = this.data.Users.Find(userId);

            this.RemoveOldAvatar(user);

            user.AvatarUrl = imageName;
            this.data.Users.Update(user);
            this.data.SaveChanges();
        }

        private void RemoveOldAvatar(User user)
        {
            if (user.AvatarUrl != GlobalConstants.DefaultUserAvatar)
            {
                var oldAvatarPath = this.Server.MapPath(GlobalConstants.ImagesPath + user.AvatarUrl);
                File.Delete(oldAvatarPath);
            }
        }
    }
}