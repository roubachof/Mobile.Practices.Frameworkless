// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SillyModel.cs" company="The Silly Company">
//   The Silly Company 2016. All rights reserved.
// </copyright>
// <summary>
//   The silly vmo.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Mobile.Practices.Frameworkless.Models
{
    public class SillyDudeModel
    {
        public SillyDudeModel(int id, string name, string role, string description, string imageUrl)
        {
            this.Id = id;
            this.Name = name;
            this.Role = role;
            this.Description = description;
            this.ImageUrl = imageUrl;
        }

        /// <summary>
        /// Gets the id.
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the role.
        /// </summary>
        public string Role { get; }

        /// <summary>
        /// Gets the description.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Gets the image url.
        /// </summary>
        public string ImageUrl { get; }
    }
}