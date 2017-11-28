using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CromulentBisgetti.ContainerPacking.Entities
{
	/// <summary>
	/// An item to be packed. Also used to hold post-packing details for the item.
	/// </summary>
	[DataContract]
	public class Item
	{
		#region Private Variables

		#endregion Private Variables

		#region Constructors
		public Item() { }
		/// <summary>
		/// Initializes a new instance of the Item class.
		/// </summary>
		/// <param name="id">The item ID.</param>
		/// <param name="length">The length of one of the three item dimensions.</param>
		/// <param name="width">The length of another of the three item dimensions.</param>
		/// <param name="height">The length of the other of the three item dimensions.</param>
		/// <param name="itemQuantity">The item quantity.</param>
		public Item(int id, decimal length, decimal width, decimal height, int quantity)
		{
			this.ID = id;
			this.Length = length;
			this.Width = width;
			this.Height = height;
			this.Quantity = quantity;
		}

		/// <summary>
		/// Initializes a new instance of the Item class by copying another item
		/// </summary>
		/// <param name="original">the Item object to copy</param>
		public Item(Item original)
		{
			ID = original.ID;
			IsPacked = original.IsPacked;
			Length = original.Length;
			Width = original.Width;
			Height = original.Height;
			CoordLength = original.CoordLength;
			CoordWidth = original.CoordWidth;
			CoordHeight = original.CoordHeight;
			Quantity = original.Quantity;
			PackLength = original.PackLength;
			PackWidth = original.PackWidth;
			PackHeight = original.PackHeight;
		}
		#endregion Constructors

		#region Public Properties

		/// <summary>
		/// Gets or sets the item ID.
		/// </summary>
		/// <value>
		/// The item ID.
		/// </value>
		[DataMember]
		public int ID { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this item has already been packed.
		/// </summary>
		/// <value>
		///   True if the item has already been packed; otherwise, false.
		/// </value>
		[DataMember]
		public bool IsPacked { get; set; }

		/// <summary>
		/// Gets or sets the length of one of the item dimensions.
		/// </summary>
		/// <value>
		/// The first item dimension.
		/// </value>
		[DataMember]
		public decimal Length { get; set; }

		/// <summary>
		/// Gets or sets the length another of the item dimensions.
		/// </summary>
		/// <value>
		/// The second item dimension.
		/// </value>
		[DataMember]
		public decimal Width { get; set; }

		/// <summary>
		/// Gets or sets the third of the item dimensions.
		/// </summary>
		/// <value>
		/// The third item dimension.
		/// </value>
		[DataMember]
		public decimal Height { get; set; }

		/// <summary>
		/// Gets or sets the x coordinate of the location of the packed item within the container.
		/// </summary>
		/// <value>
		/// The x coordinate of the location of the packed item within the container.
		/// </value>
		[DataMember]
		public decimal CoordLength { get; set; }

		/// <summary>
		/// Gets or sets the y coordinate of the location of the packed item within the container.
		/// </summary>
		/// <value>
		/// The y coordinate of the location of the packed item within the container.
		/// </value>
		[DataMember]
		public decimal CoordWidth { get; set; }

		/// <summary>
		/// Gets or sets the z coordinate of the location of the packed item within the container.
		/// </summary>
		/// <value>
		/// The z coordinate of the location of the packed item within the container.
		/// </value>
		[DataMember]
		public decimal CoordHeight { get; set; }

		/// <summary>
		/// Gets or sets the item quantity.
		/// </summary>
		/// <value>
		/// The item quantity.
		/// </value>
		[DataMember]
		public int Quantity { get; set; }

		/// <summary>
		/// Gets or sets the x dimension of the orientation of the item as it has been packed.
		/// </summary>
		/// <value>
		/// The x dimension of the orientation of the item as it has been packed.
		/// </value>
		[DataMember]
		public decimal PackLength { get; set; }

		/// <summary>
		/// Gets or sets the y dimension of the orientation of the item as it has been packed.
		/// </summary>
		/// <value>
		/// The y dimension of the orientation of the item as it has been packed.
		/// </value>
		[DataMember]
		public decimal PackWidth { get; set; }

		/// <summary>
		/// Gets or sets the z dimension of the orientation of the item as it has been packed.
		/// </summary>
		/// <value>
		/// The z dimension of the orientation of the item as it has been packed.
		/// </value>
		[DataMember]
		public decimal PackHeight { get; set; }

		/// <summary>
		/// Gets the item volume.
		/// </summary>
		/// <value>
		/// The item volume.
		/// </value>
		[DataMember]
		public decimal Volume { get { return Length * Width * Height; } }

		#endregion Public Properties
	}
}
