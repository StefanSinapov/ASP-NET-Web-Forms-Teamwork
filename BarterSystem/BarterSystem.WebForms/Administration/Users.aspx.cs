﻿using BarterSystem.Data;
using BarterSystem.WebForms.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

using BarterSystem.WebForms.Controls.Notifier;
namespace BarterSystem.WebForms.Administration
{
    public partial class Users : System.Web.UI.Page
    {
        private BarterSystemData data;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (this.User == null || !this.User.Identity.IsAuthenticated || !Roles.IsUserInRole("admin"))
            {
                Server.Transfer("~/Account/Login.aspx", true);
            }
            data = new BarterSystemData();
        }

        // The return type can be changed to IEnumerable, however to support
        // paging and sorting, the following parameters must be added:
        //     int maximumRows
        //     int startRowIndex
        //     out int totalRowCount
        //     string sortByExpression
        public IQueryable<BarterSystem.WebForms.Models.AdminUserViewModel> AdminUserLV_GetData()
        {
            //TODO filter out admins
            var users = data.Users.All()
                .Select(AdminUserViewModel.FromDataToModel);
            return users.OrderBy(x => x.Username);
        }

        // The id parameter name should match the DataKeyNames value set on the control
        public void AdminUserLV_UpdateItem(string Username)
        {
            BarterSystem.WebForms.Models.AdminUserViewModel item = null;
            var itemData = data.Users.All()
                .FirstOrDefault(x => x.UserName == Username);
            item = AdminUserViewModel.FromDataToModel.Compile()(itemData);
            if (itemData == null)
            {
                // The item wasn't found
                ModelState.AddModelError("", String.Format("Item with user name {0} was not found", Username));
                return;
            }
            TryUpdateModel(item);
            if (ModelState.IsValid)
            {
                itemData.UserName = item.Username;
                itemData.FirstName = item.FirstName;
                itemData.LastName = item.LastName;
                data.SaveChanges();
                Notifier.Success("user info updated");
            }
        }

        protected void AdminUserLV_ItemCommand(object sender, ListViewCommandEventArgs e)
        {
            if (String.Equals(e.CommandName, "Ban"))
            {
                if (!Roles.RoleExists("banned"))
                {
                    Roles.CreateRole("banned");
                }
                var userName = AdminUserLV.DataKeys[e.Item.DisplayIndex]
                    .Value.ToString();
                var user = data.Users.All()
                    .FirstOrDefault(x => x.UserName == userName);
                Roles.AddUserToRole(user.UserName, "banned");
                data.SaveChanges();
                DataBind();
                Notifier.Warning("User banned");
            }
            else if (String.Equals(e.CommandName, "Admin"))
            {
                if (!Roles.RoleExists("admin"))
                {
                    Roles.CreateRole("admin");
                }
                var userName = AdminUserLV.DataKeys[e.Item.DisplayIndex]
                    .Value.ToString();
                var user = data.Users.All()
                    .FirstOrDefault(x => x.UserName == userName);
                Roles.AddUserToRole(user.UserName, "admin");
                data.SaveChanges();
                DataBind();
                Notifier.Success("User promoted");
            }
            else if (String.Equals(e.CommandName, "LiftBan"))
            {
                var userName = AdminUserLV.DataKeys[e.Item.DisplayIndex]
                    .Value.ToString();
                var user = data.Users.All()
                    .FirstOrDefault(x => x.UserName == userName);
                Roles.RemoveUserFromRole(user.UserName, "banned");
                data.SaveChanges();
                DataBind();
                Notifier.Warning("Bann lifted");
            } 
        }
    }
}