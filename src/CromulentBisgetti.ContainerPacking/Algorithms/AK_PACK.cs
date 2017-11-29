using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CromulentBisgetti.ContainerPacking.Entities;

namespace CromulentBisgetti.ContainerPacking.Algorithms
{
    /// <summary>
    /// A 3D bin packing algorithm that is more of a bruit force recursive packing algorithm.
    /// Developed by Alan Kristensen 2017.
    /// </summary>
    public class AK_PACK : IPackingAlgorithm
    {
        #region Public Methods
        /// <summary>Runs the packing algorithm.</summary>
        /// <param name="container">The container to pack items into.</param>
        /// <param name="items">The items to pack.</param>
        /// <returns>The bin packing result.</returns>
        public AlgorithmPackingResult Run(Container container, List<Item> items)
        {
            //Initialize object list
            List<Item> itemsToPack = new List<Item>();
            //Save the passed in variables
            this.container = container;
            //split the items into one item per entry (Quantity = 1) with the largest item being first
            foreach (var item in items.Where(x => x.Quantity > 0).OrderByDescending(x => x.Volume))
                for(int i = item.Quantity; i > 0; i--)
                    itemsToPack.Add(new Item(item.ID, item.Length, item.Width, item.Height, 1));

            var results = FillContainerItemFirst(new List<Item>(),
                                                 itemsToPack, 
                                                 null
                                                );

            var result = new AlgorithmPackingResult()
            {
                AlgorithmID = (int)AlgorithmType.AK_PACK,
                AlgorithmName = "AK_PACK",
                IsCompletePack = results.All(x => x.IsPacked),
                PackedItems = results.Where(x => x.IsPacked).ToList(),
                UnpackedItems = results.Where(x => !x.IsPacked).ToList()
            };

            return result;
        }
        #endregion Public Methods

        #region Private Variables

        private Container container;

        /// <summary>A list of orientations to use.</summary>
        private enum OrientationEnum
        {
            LWH,
            LHW,
            WLH,
            WHL,
            HLW,
            HWL
        }

        #endregion Private Variables

        #region Private Recursive Methods

        /// <summary>pack the items in the container in order of the unPackedItems list</summary>
        /// <param name="packedItems">a list of Items already packed</param>
        /// <param name="itemToPack">an item to pack in the box next.  NOT PART OF UNPACKED ITEMS.</param>
        /// <param name="unPackedItems">a list of Items still to pack in the box</param>
        /// <returns></returns>
        private List<Item> FillContainerItemFirst(List<Item> packedItems, List<Item> unPackedItems, Item itemToPack)
        {
            //Make sure packed items are not null
            packedItems = packedItems ?? new List<Item>();

            //Try to pack it only if there is an item there
            if (itemToPack != null)
                packedItems.Add(PlaceItemInContainer(itemToPack, packedItems));

            //Try to pack the rest of the items
            if (unPackedItems != null && unPackedItems.Count() > 0)
            {
                //Try to fit the remaining items until an orientation works
                List<Item> bestResult = null;

                //try each of the orientations for the extracted item
                foreach (var orient in Enum.GetValues(typeof(OrientationEnum)).Cast<OrientationEnum>())
                {
                    List<Item> nextPackedItems = new List<Item>(packedItems);
                    List<Item> nextUnPackedItems = unPackedItems.Skip(1).ToList();
                    Item nextItemToPack = new Item(unPackedItems.FirstOrDefault());

                    //Orient the item
                    SetOrientation(nextItemToPack, orient);

                    //Try to pack it and the remaining items
                    var result = FillContainerItemFirst(nextPackedItems, 
                                                        nextUnPackedItems, 
                                                        nextItemToPack);
                    //if successful, save the result and return it
                    if (result.All(x => x.IsPacked))
                    {
                        if (bestResult == null || (GetFillRank(result) > GetFillRank(bestResult)))
                            bestResult = result;
                    }
                    else if (bestResult == null || (result != null && GetFillRank(result) > GetFillRank(bestResult)))
                    {
                        bestResult = result;
                    }
                }

                if (bestResult != null)
                    packedItems = bestResult;
            }

            //return the fully packed container
            return packedItems;
        }

        #endregion Recursive Methods

        #region Private Methods

        /// <summary>See if we can fit an item in with the packageItems</summary>
        /// <param name="itemToPlace">The item to place in the container</param>
        /// <param name="packedItems">The list of items already in the container</param>
        /// <returns>The updated item to be placed in the container</returns>
        private Item PlaceItemInContainer(Item itemToPlace, List<Item> packedItems)
        {
            for (int placeHeight = 0; placeHeight < container.Height; placeHeight++)
            { 
                for (int placeWidth = 0; placeWidth < container.Width; placeWidth++)
                {
                    for (int placeLength = 0; placeLength < container.Length; placeLength++)
                    {
                        bool obstructed = false;

                        //Does the item fit in the container at the location it is at
                        if (((placeLength + itemToPlace.PackLength) <= container.Length) &&  //itemToPlace's left face is to the left of the containers right face
                            ((placeWidth  + itemToPlace.PackWidth ) <= container.Width ) &&  //itemToPlace's top face is below the containers top face
                            ((placeHeight + itemToPlace.PackHeight) <= container.Height)     //itemToPlace's front face is behind the containers front face
                           )
                        {   //The item can fit in the container
                            //Does an item occupy this starting spot?
                            foreach (var itemToCheck in packedItems.Where(x => x.IsPacked))
                            {
                                /******************************************************************************\
                                *  This assumes axis aligned packages (no arbitrary rotation of items in box)  *
                                *  boxes may touch, but not overlap.                                           *
                                \******************************************************************************/
                                if (!(
                                      (placeLength             >= (itemToCheck.CoordLength + itemToCheck.PackLength)) ||  //itemToPlace's left face is to the right of the itemToCheck's right face
                                      (itemToCheck.CoordLength >= (placeLength             + itemToPlace.PackLength)) ||  //itemToPlace's right face is to the left of the itemToCheck's left face
                                      (placeWidth              >= (itemToCheck.CoordWidth  + itemToCheck.PackWidth )) ||  //itemToPlace's back face is in front of itemToCheck's front face
                                      (itemToCheck.CoordWidth  >= (placeWidth              + itemToPlace.PackWidth )) ||  //itemToPlace's front face is behind itemToCheck's back face
                                      (placeHeight             >= (itemToCheck.CoordHeight + itemToCheck.PackHeight)) ||  //itemToPlace's top face is below itemToCheck's bottom face
                                      (itemToCheck.CoordHeight >= (placeHeight             + itemToPlace.PackHeight))     //itemToPlace's bottom face is above itemToCheck's top face
                                     )
                                   )
                                {   // items do overlap
                                    obstructed = true;
                                    //increment placeX to the right of the itemToCheck
                                    if (placeLength < (itemToCheck.CoordLength + itemToCheck.PackLength))
                                        placeLength = (int)Math.Ceiling(itemToCheck.CoordLength + itemToCheck.PackLength)-1;
                                    //skip checking the rest of the packedItems
                                    break;
                                }
                            }
                            if (!obstructed)
                            {
                                itemToPlace.CoordLength = placeLength;
                                itemToPlace.CoordWidth  = placeWidth ;
                                itemToPlace.CoordHeight = placeHeight;
                                itemToPlace.IsPacked = true;
                                return itemToPlace;
                            }
                        }
                        else
                        {
                            //TODO: be smarter about incrementing the placeLength, placeWidth, and placeHeight when the item does not fit
                        }
                    }
                }
            }
            //Item did not find a place to be, so return it unadded
            return itemToPlace;
        }

        /// <summary>The fill rank is the sum of the following:
        ///     +100.0 = if all items are packed
        ///     +xxx.0 = the xxx% filled space
        /// </summary>
        /// <param name="items">the Items array to check</param>
        /// <returns>0 - 200 integer value</returns>
        private int GetFillRank(List<Item> items)
        {
            decimal containerVolume = container.Length * container.Width * container.Height;
            if (containerVolume == 0) return 0;
            decimal volumePacked = items.Where(item => item.IsPacked).Sum(item => item.Volume);

            return ((items.All(x => x.IsPacked)) ? 100 : 0)                 //Everything Packed
                 + (int)Math.Ceiling(volumePacked / containerVolume * 100)  //% container free space
                 ;
        }

        /// <summary>Set the orientation of the packed item</summary>
        /// <param name="itemToPack">The Item object to adjust</param>
        /// <param name="orientation">The orientation to set</param>
        private void SetOrientation (Item itemToPack, OrientationEnum orientation)
        {
            itemToPack.PackLength = (orientation == OrientationEnum.LWH || orientation == OrientationEnum.LHW)
                                    ? itemToPack.Length
                                    : (orientation == OrientationEnum.WHL || orientation == OrientationEnum.WLH)
                                      ? itemToPack.Width
                                      : itemToPack.Height;
            itemToPack.PackWidth = (orientation == OrientationEnum.HLW || orientation == OrientationEnum.WLH)
                                    ? itemToPack.Length
                                    : (orientation == OrientationEnum.LWH || orientation == OrientationEnum.HWL)
                                      ? itemToPack.Width
                                      : itemToPack.Height;
            itemToPack.PackHeight = (orientation == OrientationEnum.HWL || orientation == OrientationEnum.WHL)
                                    ? itemToPack.Length
                                    : (orientation == OrientationEnum.HLW || orientation == OrientationEnum.LHW)
                                      ? itemToPack.Width
                                      : itemToPack.Height;
        }

        #endregion Private Methods
    }
}
