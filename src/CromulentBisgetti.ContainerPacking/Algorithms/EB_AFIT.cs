﻿using CromulentBisgetti.ContainerPacking.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CromulentBisgetti.ContainerPacking.Algorithms
{
	/// <summary>
	/// A 3D bin packing algorithm originally ported from https://github.com/keremdemirer/3dbinpackingjs,
	/// which itself was a JavaScript port of https://github.com/wknechtel/3d-bin-pack/, which is a C reconstruction 
	/// of a novel algorithm developed in a U.S. Air Force master's thesis by Erhan Baltacioglu in 2001.
	/// http://www.dtic.mil/dtic/tr/fulltext/u2/a391201.pdf
	/// </summary>
	public class EB_AFIT : IPackingAlgorithm
	{
		#region Public Methods

		/// <summary>
		/// Runs the packing algorithm.
		/// </summary>
		/// <param name="container">The container to pack items into.</param>
		/// <param name="items">The items to pack.</param>
		/// <returns>The bin packing result.</returns>
		public AlgorithmPackingResult Run(Container container, List<Item> items)
		{
			this.container = container;

			Initialize(items);
			ExecuteIterations();
			Report();

			AlgorithmPackingResult result = new AlgorithmPackingResult();
			result.AlgorithmID = (int)AlgorithmType.EB_AFIT;
			result.AlgorithmName = "EB-AFIT";

			for (int i = 1; i <= itemsToPackCount; i++)
			{
				itemsToPack[i].Quantity = 1;

				if (!itemsToPack[i].IsPacked)
				{
					result.UnpackedItems.Add(itemsToPack[i]);
				}
			}

			result.PackedItems = itemsPackedInOrder;
			


			if (result.UnpackedItems.Count == 0)
			{
				result.IsCompletePack = true;
			}

			return result;
		}

		#endregion Public Methods

		#region Private Variables

		private Container container;
		private List<Item> itemsToPack;
		private List<Item> itemsPackedInOrder;
		private List<Layer> layers;
		private ContainerPackingResult result;

		private ScrapPad scrapfirst;
		private ScrapPad scrapmemb;
		private ScrapPad smallestZ;
		private ScrapPad trash;

		private bool evened;
		private bool hundredPercentPacked = false;
		private bool layerDone;
		private bool packing;
		private bool packingBest = false;
		private bool quit = false;
		private bool unpacked;

		private int bboxi;
		private int bestIteration;
		private int bestPackedItemCount;
		private int bestVariant;
		private int boxi;
		private int cboxi;
		private int itelayer;
		private int iterationsCount;
		private int layerListLen;
		private int layersIndex;
		private int n;
		private int packedItemCount;
		private int variant;
		private int x;

		private decimal bbfx;
		private decimal bbfy;
		private decimal bbfz;
		private decimal bboxx;
		private decimal bboxy;
		private decimal bboxz;
		private decimal bestVolume;
		private decimal bfx;
		private decimal bfy;
		private decimal bfz;
		private decimal boxx;
		private decimal boxy;
		private decimal boxz;
		private decimal cboxx;
		private decimal cboxy;
		private decimal cboxz;
		private decimal layerinlayer;
		private decimal layerThickness;
		private decimal lilz;
		private decimal packedVolume;
		private decimal packedy;
		private decimal percentagePackedItemsByVolume;
		private decimal percentageContainerUsed;
		private decimal prelayer;
		private decimal prepackedy;
		private decimal preremainpy;
		private decimal px;
		private decimal py;
		private decimal pz;
		private decimal remainpy;
		private decimal remainpz;
		private decimal strcox;
		private decimal strcoy;
		private decimal strcoz;
		private decimal strpackx;
		private decimal strpacky;
		private decimal strpackz;
		private decimal itemsToPackCount;
		private decimal totalItemVolume;
		private decimal totalContainerVolume;

		#endregion Private Variables

		#region Private Methods

		/// <summary>
		/// Analyzes each unpacked box to find the best fitting one to the empty space given.
		/// </summary>
		private void AnalyzeBox(decimal hmx, decimal hy, decimal hmy, decimal hz, decimal hmz, decimal dim1, decimal dim2, decimal dim3)
		{
			if (dim1 <= hmx && dim2 <= hmy && dim3 <= hmz)
			{
				if (dim2 <= hy)
				{
					if (hy - dim2 < bfy)
					{
						boxx = dim1;
						boxy = dim2;
						boxz = dim3;
						bfx = hmx - dim1;
						bfy = hy - dim2;
						bfz = Math.Abs(hz - dim3);
						boxi = x;
					}
					else if (hy - dim2 == bfy && hmx - dim1 < bfx)
					{
						boxx = dim1;
						boxy = dim2;
						boxz = dim3;
						bfx = hmx - dim1;
						bfy = hy - dim2;
						bfz = Math.Abs(hz - dim3);
						boxi = x;
					}
					else if (hy - dim2 == bfy && hmx - dim1 == bfx && Math.Abs(hz - dim3) < bfz)
					{
						boxx = dim1;
						boxy = dim2;
						boxz = dim3;
						bfx = hmx - dim1;
						bfy = hy - dim2;
						bfz = Math.Abs(hz - dim3);
						boxi = x;
					}
				}
				else
				{
					if (dim2 - hy < bbfy)
					{
						bboxx = dim1;
						bboxy = dim2;
						bboxz = dim3;
						bbfx = hmx - dim1;
						bbfy = dim2 - hy;
						bbfz = Math.Abs(hz - dim3);
						bboxi = x;
					}
					else if (dim2 - hy == bbfy && hmx - dim1 < bbfx)
					{
						bboxx = dim1;
						bboxy = dim2;
						bboxz = dim3;
						bbfx = hmx - dim1;
						bbfy = dim2 - hy;
						bbfz = Math.Abs(hz - dim3);
						bboxi = x;
					}
					else if (dim2 - hy == bbfy && hmx - dim1 == bbfx && Math.Abs(hz - dim3) < bbfz)
					{
						bboxx = dim1;
						bboxy = dim2;
						bboxz = dim3;
						bbfx = hmx - dim1;
						bbfy = dim2 - hy;
						bbfz = Math.Abs(hz - dim3);
						bboxi = x;
					}
				}
			}
		}

		/// <summary>
		/// After finding each box, the candidate boxes and the condition of the layer are examined.
		/// </summary>
		private void CheckFound()
		{
			evened = false;

			if (boxi != 0)
			{
				cboxi = boxi;
				cboxx = boxx;
				cboxy = boxy;
				cboxz = boxz;
			}
			else
			{
				if ((bboxi > 0) && (layerinlayer != 0 || (smallestZ.Pre == null && smallestZ.Post == null)))
				{
					if (layerinlayer == 0)
					{
						prelayer = layerThickness;
						lilz = smallestZ.CumZ;
					}

					cboxi = bboxi;
					cboxx = bboxx;
					cboxy = bboxy;
					cboxz = bboxz;
					layerinlayer = layerinlayer + bboxy - layerThickness;
					layerThickness = bboxy;
				}
				else
				{
					if (smallestZ.Pre == null && smallestZ.Post == null)
					{
						layerDone = true;
					}
					else
					{
						evened = true;

						if (smallestZ.Pre == null)
						{
							trash = smallestZ.Post;
							smallestZ.CumX = smallestZ.Post.CumX;
							smallestZ.CumZ = smallestZ.Post.CumZ;
							smallestZ.Post = smallestZ.Post.Post;
							if (smallestZ.Post != null)
							{
								smallestZ.Post.Pre = smallestZ;
							}
						}
						else if (smallestZ.Post == null)
						{
							smallestZ.Pre.Post = null;
							smallestZ.Pre.CumX = smallestZ.CumX;
						}
						else
						{
							if (smallestZ.Pre.CumZ == smallestZ.Post.CumZ)
							{
								smallestZ.Pre.Post = smallestZ.Post.Post;

								if (smallestZ.Post.Post != null)
								{
									smallestZ.Post.Post.Pre = smallestZ.Pre;
								}

								smallestZ.Pre.CumX = smallestZ.Post.CumX;
							}
							else
							{
								smallestZ.Pre.Post = smallestZ.Post;
								smallestZ.Post.Pre = smallestZ.Pre;

								if (smallestZ.Pre.CumZ < smallestZ.Post.CumZ)
								{
									smallestZ.Pre.CumX = smallestZ.CumX;
								}
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// Executes the packing algorithm variants.
		/// </summary>
		private void ExecuteIterations()
		{
			for (variant = 1; (variant <= 6) && !quit; variant++)
			{
				switch (variant)
				{
					case 1:
						px = container.Length; py = container.Height; pz = container.Width;
						break;

					case 2:
						px = container.Width; py = container.Height; pz = container.Length;
						break;

					case 3:
						px = container.Width; py = container.Length; pz = container.Height;
						break;

					case 4:
						px = container.Height; py = container.Length; pz = container.Width;
						break;

					case 5:
						px = container.Length; py = container.Width; pz = container.Height;
						break;

					case 6:
						px = container.Height; py = container.Width; pz = container.Length;
						break;
				}

				layers.Add(new Layer { LayerEval = -1 });
				ListCanditLayers();
				layers = layers.OrderBy(l => l.LayerEval).ToList();

				for (layersIndex = 1; (layersIndex <= layerListLen) && !quit; layersIndex++)
				{
					++iterationsCount;

					//printf("VARIANT: " + variant + "; ITERATION (TOTAL): " + itenum + "; BEST SO FAR: " + percentageused + " %%;");
					packedVolume = 0.0M;
					packedy = 0;
					packing = true;
					layerThickness = layers[layersIndex].LayerDim;
					itelayer = layersIndex;
					remainpy = py;
					remainpz = pz;
					packedItemCount = 0;

					for (x = 1; x <= itemsToPackCount; x++)
					{
						itemsToPack[x].IsPacked = false;
					}

					do
					{
						layerinlayer = 0;
						layerDone = false;

						PackLayer();

						packedy = packedy + layerThickness;
						remainpy = py - packedy;

						if (layerinlayer != 0 && !quit)
						{
							prepackedy = packedy;
							preremainpy = remainpy;
							remainpy = layerThickness - prelayer;
							packedy = packedy - layerThickness + prelayer;
							remainpz = lilz;
							layerThickness = layerinlayer;
							layerDone = false;

							PackLayer();

							packedy = prepackedy;
							remainpy = preremainpy;
							remainpz = pz;
						}

						FindLayer(remainpy);
					} while (packing && !quit);

					if ((packedVolume > bestVolume) && !quit)
					{
						bestVolume = packedVolume;
						bestVariant = variant;
						bestIteration = itelayer;
						bestPackedItemCount = packedItemCount;
					}

					if (hundredPercentPacked) break;

					percentageContainerUsed = bestVolume * 100 / totalContainerVolume;
				}

				if (hundredPercentPacked) break;

				if ((container.Length == container.Height) && (container.Height == container.Width)) variant = 6;
			}
		}

		/// <summary>
		/// Finds the most proper boxes by looking at all six possible orientations,
		/// empty space given, adjacent boxes, and pallet limits.
		/// </summary>
		private void FindBox(decimal hmx, decimal hy, decimal hmy, decimal hz, decimal hmz)
		{
			int y;
			bfx = 32767;
			bfy = 32767;
			bfz = 32767;
			bbfx = 32767;
			bbfy = 32767;
			bbfz = 32767;
			boxi = 0;
			bboxi = 0;

			for (y = 1; y <= itemsToPackCount; y = y + itemsToPack[y].Quantity)
			{
				for (x = y; x < x + itemsToPack[y].Quantity - 1; x++)
				{
					if (!itemsToPack[x].IsPacked) break;
				}

				if (itemsToPack[x].IsPacked) continue;

				if (x > itemsToPackCount) return;

				AnalyzeBox(hmx, hy, hmy, hz, hmz, itemsToPack[x].Length, itemsToPack[x].Width, itemsToPack[x].Height);

				if ((itemsToPack[x].Length == itemsToPack[x].Height) && (itemsToPack[x].Height == itemsToPack[x].Width)) continue;

				AnalyzeBox(hmx, hy, hmy, hz, hmz, itemsToPack[x].Length, itemsToPack[x].Height, itemsToPack[x].Width);
				AnalyzeBox(hmx, hy, hmy, hz, hmz, itemsToPack[x].Width, itemsToPack[x].Length, itemsToPack[x].Height);
				AnalyzeBox(hmx, hy, hmy, hz, hmz, itemsToPack[x].Width, itemsToPack[x].Height, itemsToPack[x].Length);
				AnalyzeBox(hmx, hy, hmy, hz, hmz, itemsToPack[x].Height, itemsToPack[x].Length, itemsToPack[x].Width);
				AnalyzeBox(hmx, hy, hmy, hz, hmz, itemsToPack[x].Height, itemsToPack[x].Width, itemsToPack[x].Length);
			}
		}

		/// <summary>
		/// Finds the most proper layer height by looking at the unpacked boxes and the remaining empty space available.
		/// </summary>
		private void FindLayer(decimal thickness)
		{
			decimal exdim = 0;
			decimal dimdif;
			decimal dimen2 = 0;
			decimal dimen3 = 0;
			int y;
			int z;
			decimal layereval;
			decimal eval;
			layerThickness = 0;
			eval = 1000000;

			for (x = 1; x <= itemsToPackCount; x++)
			{
				if (itemsToPack[x].IsPacked) continue;

				for (y = 1; y <= 3; y++)
				{
					switch (y)
					{
						case 1:
							exdim = itemsToPack[x].Length;
							dimen2 = itemsToPack[x].Width;
							dimen3 = itemsToPack[x].Height;
							break;

						case 2:
							exdim = itemsToPack[x].Width;
							dimen2 = itemsToPack[x].Length;
							dimen3 = itemsToPack[x].Height;
							break;

						case 3:
							exdim = itemsToPack[x].Height;
							dimen2 = itemsToPack[x].Length;
							dimen3 = itemsToPack[x].Width;
							break;
					}

					layereval = 0;

					if ((exdim <= thickness) && (((dimen2 <= px) && (dimen3 <= pz)) || ((dimen3 <= px) && (dimen2 <= pz))))
					{
						for (z = 1; z <= itemsToPackCount; z++)
						{
							if (!(x == z) && !(itemsToPack[z].IsPacked))
							{
								dimdif = Math.Abs(exdim - itemsToPack[z].Length);

								if (Math.Abs(exdim - itemsToPack[z].Width) < dimdif)
								{
									dimdif = Math.Abs(exdim - itemsToPack[z].Width);
								}

								if (Math.Abs(exdim - itemsToPack[z].Height) < dimdif)
								{
									dimdif = Math.Abs(exdim - itemsToPack[z].Height);
								}

								layereval = layereval + dimdif;
							}
						}

						if (layereval < eval)
						{
							eval = layereval;
							layerThickness = exdim;
						}
					}
				}
			}

			if (layerThickness == 0 || layerThickness > remainpy) packing = false;
		}

		/// <summary>
		/// Finds the first to be packed gap in the layer edge.
		/// </summary>
		private void FindSmallestZ()
		{
			scrapmemb = scrapfirst;
			smallestZ = scrapmemb;

			while (scrapmemb.Post != null)
			{
				if (scrapmemb.Post.CumZ < smallestZ.CumZ)
				{
					smallestZ = scrapmemb.Post;
				}

				scrapmemb = scrapmemb.Post;
			}
		}

		/// <summary>
		/// Data for the visualization program is written to the "visudat" file and
		/// the list of unpacked boxes is merged to the end of the report file.
		/// </summary>
		private void GraphUnpackedOut()
		{
			int n = 0;

			if (!unpacked)
			{
				strcox = itemsToPack[cboxi].CoordLength;
				strcoy = itemsToPack[cboxi].CoordWidth;
				strcoz = itemsToPack[cboxi].CoordHeight;
				strpackx = itemsToPack[cboxi].PackLength;
				strpacky = itemsToPack[cboxi].PackHeight;
				strpackz = itemsToPack[cboxi].PackWidth;
			}
			else
			{
				n = cboxi;
				strpackx = itemsToPack[cboxi].Length;
				strpacky = itemsToPack[cboxi].Height;
				strpackz = itemsToPack[cboxi].Width;
			}
			if (!unpacked)
			{
				//Print(strcox, strcoy, strcoz, strpackx, strpacky, strpackz);
			}
			else
			{
				Print(n, strpackx, strpacky, strpackz);
			}
		}

		/// <summary>
		/// Initializes everything.
		/// </summary>
		private void Initialize(List<Item> items)
		{
			itemsToPack = new List<Item>();
			itemsPackedInOrder = new List<Item>();
			result = new ContainerPackingResult();

			// The original code uses 1-based indexing everywhere. This fake entry is added to the beginning
			// of the list to make that possible.
			itemsToPack.Add(new Item(0, 0, 0, 0, 0));

			layers = new List<Layer>();
			itemsToPackCount = 0;

			foreach (Item item in items)
			{
				for (int i = 1; i <= item.Quantity; i++)
				{
					Item newItem = new Item(item.ID, item.Length, item.Width, item.Height, item.Quantity);
					itemsToPack.Add(newItem);
				}

				itemsToPackCount += item.Quantity;
			}

			itemsToPack.Add(new Item(0, 0, 0, 0, 0));

			totalContainerVolume = container.Length * container.Height * container.Width;
			totalItemVolume = 0.0M;

			for (x = 1; x <= itemsToPackCount; x++)
			{
				totalItemVolume = totalItemVolume + itemsToPack[x].Volume;
			}

			scrapfirst = new ScrapPad();

			scrapfirst.Pre = null;
			scrapfirst.Post = null;
			bestVolume = 0.0M;
			packingBest = false;
			hundredPercentPacked = false;
			iterationsCount = 0;
			quit = false;
		}

		/// <summary>
		/// Lists all possible layer heights by giving a weight value to each of them.
		/// </summary>
		private void ListCanditLayers()
		{
			bool same;
			decimal exdim = 0;
			decimal dimdif;
			decimal dimen2 = 0;
			decimal dimen3 = 0;
			int y;
			int z;
			int k;
			decimal layereval;

			layerListLen = 0;

			for (x = 1; x <= itemsToPackCount; x++)
			{
				for (y = 1; y <= 3; y++)
				{
					switch (y)
					{
						case 1:
							exdim = itemsToPack[x].Length;
							dimen2 = itemsToPack[x].Width;
							dimen3 = itemsToPack[x].Height;
							break;

						case 2:
							exdim = itemsToPack[x].Width;
							dimen2 = itemsToPack[x].Length;
							dimen3 = itemsToPack[x].Height;
							break;

						case 3:
							exdim = itemsToPack[x].Height;
							dimen2 = itemsToPack[x].Length;
							dimen3 = itemsToPack[x].Width;
							break;
					}

					if ((exdim > py) || (((dimen2 > px) || (dimen3 > pz)) && ((dimen3 > px) || (dimen2 > pz)))) continue;

					same = false;

					for (k = 1; k <= layerListLen; k++)
					{
						if (exdim == layers[k].LayerDim)
						{
							same = true;
							continue;
						}
					}

					if (same) continue;

					layereval = 0;

					for (z = 1; z <= itemsToPackCount; z++)
					{
						if (!(x == z))
						{
							dimdif = Math.Abs(exdim - itemsToPack[z].Length);

							if (Math.Abs(exdim - itemsToPack[z].Width) < dimdif)
							{
								dimdif = Math.Abs(exdim - itemsToPack[z].Width);
							}
							if (Math.Abs(exdim - itemsToPack[z].Height) < dimdif)
							{
								dimdif = Math.Abs(exdim - itemsToPack[z].Height);
							}
							layereval = layereval + dimdif;
						}
					}

					layerListLen++;

					layers.Add(new Layer());
					layers[layerListLen].LayerEval = layereval;
					layers[layerListLen].LayerDim = exdim;
				}
			}
		}

		/// <summary>
		/// Transforms the found coordinate system to the one entered by the user and writes them
		/// to the report file.
		/// </summary>
		private void OutputBoxList()
		{
			int strx;
			bool strpackst;
			decimal strdim1;
			decimal strdim2;
			decimal strdim3;
			dynamic strcox;
			dynamic strcoy;
			dynamic strcoz;
			dynamic strpackx;
			dynamic strpacky;
			dynamic strpackz;

			dynamic x = 0;
			dynamic y = 0;
			dynamic z = 0;
			dynamic bx = 0;
			dynamic by = 0;
			dynamic bz = 0;

			switch (bestVariant)
			{
				case 1:
					x = itemsToPack[cboxi].CoordLength;
					y = itemsToPack[cboxi].CoordWidth;
					z = itemsToPack[cboxi].CoordHeight;
					bx = itemsToPack[cboxi].PackLength;
					by = itemsToPack[cboxi].PackWidth;
					bz = itemsToPack[cboxi].PackHeight;
					break;

				case 2:
					x = itemsToPack[cboxi].CoordHeight;
					y = itemsToPack[cboxi].CoordWidth;
					z = itemsToPack[cboxi].CoordLength;
					bx = itemsToPack[cboxi].PackHeight;
					by = itemsToPack[cboxi].PackWidth;
					bz = itemsToPack[cboxi].PackLength;
					break;

				case 3:
					x = itemsToPack[cboxi].CoordWidth;
					y = itemsToPack[cboxi].CoordHeight;
					z = itemsToPack[cboxi].CoordLength;
					bx = itemsToPack[cboxi].PackWidth;
					by = itemsToPack[cboxi].PackHeight;
					bz = itemsToPack[cboxi].PackLength;
					break;

				case 4:
					x = itemsToPack[cboxi].CoordWidth;
					y = itemsToPack[cboxi].CoordLength;
					z = itemsToPack[cboxi].CoordHeight;
					bx = itemsToPack[cboxi].PackWidth;
					by = itemsToPack[cboxi].PackLength;
					bz = itemsToPack[cboxi].PackHeight;
					break;

				case 5:
					x = itemsToPack[cboxi].CoordLength;
					y = itemsToPack[cboxi].CoordHeight;
					z = itemsToPack[cboxi].CoordWidth;
					bx = itemsToPack[cboxi].PackLength;
					by = itemsToPack[cboxi].PackHeight;
					bz = itemsToPack[cboxi].PackWidth;
					break;

				case 6:
					x = itemsToPack[cboxi].CoordHeight;
					y = itemsToPack[cboxi].CoordLength;
					z = itemsToPack[cboxi].CoordWidth;
					bx = itemsToPack[cboxi].PackHeight;
					by = itemsToPack[cboxi].PackLength;
					bz = itemsToPack[cboxi].PackWidth;
					break;
			}

			strx = cboxi;
			strpackst = itemsToPack[cboxi].IsPacked;
			strdim1 = itemsToPack[cboxi].Length;
			strdim2 = itemsToPack[cboxi].Width;
			strdim3 = itemsToPack[cboxi].Height;
			strcox = x;
			strcoy = y;
			strcoz = z;
			strpackx = bx;
			strpacky = by;
			strpackz = bz;

			itemsToPack[cboxi].CoordLength = x;
			itemsToPack[cboxi].CoordWidth = y;
			itemsToPack[cboxi].CoordHeight = z;
			itemsToPack[cboxi].PackLength = bx;
			itemsToPack[cboxi].PackHeight = by;
			itemsToPack[cboxi].PackWidth = bz;

			itemsPackedInOrder.Add(itemsToPack[cboxi]);
			Print(strx, strpackst, strdim1, strdim2, strdim3, strcox, strcoy, strcoz, strpackx, strpacky, strpackz);
		}

		/// <summary>
		/// Packs the boxes found and arranges all variables and records properly.
		/// </summary>
		private void PackLayer()
		{
			decimal lenx;
			decimal lenz;
			decimal lpz;

			if (layerThickness == 0)
			{
				packing = false;
				return;
			}

			scrapfirst.CumX = px;
			scrapfirst.CumZ = 0;

			for (; !quit;)
			{
				FindSmallestZ();

				if ((smallestZ.Pre == null) && (smallestZ.Post == null))
				{
					//*** SITUATION-1: NO BOXES ON THE RIGHT AND LEFT SIDES ***

					lenx = smallestZ.CumX;
					lpz = remainpz - smallestZ.CumZ;
					FindBox(lenx, layerThickness, remainpy, lpz, lpz);
					CheckFound();

					if (layerDone) break;
					if (evened) continue;

					itemsToPack[cboxi].CoordLength = 0;
					itemsToPack[cboxi].CoordWidth = packedy;
					itemsToPack[cboxi].CoordHeight = smallestZ.CumZ;
					if (cboxx == smallestZ.CumX)
					{
						smallestZ.CumZ = smallestZ.CumZ + cboxz;
					}
					else
					{
						smallestZ.Post = new ScrapPad();

						smallestZ.Post.Post = null;
						smallestZ.Post.Pre = smallestZ;
						smallestZ.Post.CumX = smallestZ.CumX;
						smallestZ.Post.CumZ = smallestZ.CumZ;
						smallestZ.CumX = cboxx;
						smallestZ.CumZ = smallestZ.CumZ + cboxz;
					}

					VolumeCheck();
				}
				else if (smallestZ.Pre == null)
				{
					//*** SITUATION-2: NO BOXES ON THE LEFT SIDE ***

					lenx = smallestZ.CumX;
					lenz = smallestZ.Post.CumZ - smallestZ.CumZ;
					lpz = remainpz - smallestZ.CumZ;
					FindBox(lenx, layerThickness, remainpy, lenz, lpz);
					CheckFound();

					if (layerDone) break;
					if (evened) continue;

					itemsToPack[cboxi].CoordWidth = packedy;
					itemsToPack[cboxi].CoordHeight = smallestZ.CumZ;
					if (cboxx == smallestZ.CumX)
					{
						itemsToPack[cboxi].CoordLength = 0;

						if (smallestZ.CumZ + cboxz == smallestZ.Post.CumZ)
						{
							smallestZ.CumZ = smallestZ.Post.CumZ;
							smallestZ.CumX = smallestZ.Post.CumX;
							trash = smallestZ.Post;
							smallestZ.Post = smallestZ.Post.Post;

							if (smallestZ.Post != null)
							{
								smallestZ.Post.Pre = smallestZ;
							}
						}
						else
						{
							smallestZ.CumZ = smallestZ.CumZ + cboxz;
						}
					}
					else
					{
						itemsToPack[cboxi].CoordLength = smallestZ.CumX - cboxx;

						if (smallestZ.CumZ + cboxz == smallestZ.Post.CumZ)
						{
							smallestZ.CumX = smallestZ.CumX - cboxx;
						}
						else
						{
							smallestZ.Post.Pre = new ScrapPad();

							smallestZ.Post.Pre.Post = smallestZ.Post;
							smallestZ.Post.Pre.Pre = smallestZ;
							smallestZ.Post = smallestZ.Post.Pre;
							smallestZ.Post.CumX = smallestZ.CumX;
							smallestZ.CumX = smallestZ.CumX - cboxx;
							smallestZ.Post.CumZ = smallestZ.CumZ + cboxz;
						}
					}

					VolumeCheck();
				}
				else if (smallestZ.Post == null)
				{
					//*** SITUATION-3: NO BOXES ON THE RIGHT SIDE ***

					lenx = smallestZ.CumX - smallestZ.Pre.CumX;
					lenz = smallestZ.Pre.CumZ - smallestZ.CumZ;
					lpz = remainpz - smallestZ.CumZ;
					FindBox(lenx, layerThickness, remainpy, lenz, lpz);
					CheckFound();

					if (layerDone) break;
					if (evened) continue;

					itemsToPack[cboxi].CoordWidth = packedy;
					itemsToPack[cboxi].CoordHeight = smallestZ.CumZ;
					itemsToPack[cboxi].CoordLength = smallestZ.Pre.CumX;

					if (cboxx == smallestZ.CumX - smallestZ.Pre.CumX)
					{
						if (smallestZ.CumZ + cboxz == smallestZ.Pre.CumZ)
						{
							smallestZ.Pre.CumX = smallestZ.CumX;
							smallestZ.Pre.Post = null;
						}
						else
						{
							smallestZ.CumZ = smallestZ.CumZ + cboxz;
						}
					}
					else
					{
						if (smallestZ.CumZ + cboxz == smallestZ.Pre.CumZ)
						{
							smallestZ.Pre.CumX = smallestZ.Pre.CumX + cboxx;
						}
						else
						{
							smallestZ.Pre.Post = new ScrapPad();

							smallestZ.Pre.Post.Pre = smallestZ.Pre;
							smallestZ.Pre.Post.Post = smallestZ;
							smallestZ.Pre = smallestZ.Pre.Post;
							smallestZ.Pre.CumX = smallestZ.Pre.Pre.CumX + cboxx;
							smallestZ.Pre.CumZ = smallestZ.CumZ + cboxz;
						}
					}

					VolumeCheck();
				}
				else if (smallestZ.Pre.CumZ == smallestZ.Post.CumZ)
				{
					//*** SITUATION-4: THERE ARE BOXES ON BOTH OF THE SIDES ***

					//*** SUBSITUATION-4A: SIDES ARE EQUAL TO EACH OTHER ***

					lenx = smallestZ.CumX - smallestZ.Pre.CumX;
					lenz = smallestZ.Pre.CumZ - smallestZ.CumZ;
					lpz = remainpz - smallestZ.CumZ;

					FindBox(lenx, layerThickness, remainpy, lenz, lpz);
					CheckFound();

					if (layerDone) break;
					if (evened) continue;

					itemsToPack[cboxi].CoordWidth = packedy;
					itemsToPack[cboxi].CoordHeight = smallestZ.CumZ;

					if (cboxx == smallestZ.CumX - smallestZ.Pre.CumX)
					{
						itemsToPack[cboxi].CoordLength = smallestZ.Pre.CumX;

						if (smallestZ.CumZ + cboxz == smallestZ.Post.CumZ)
						{
							smallestZ.Pre.CumX = smallestZ.Post.CumX;

							if (smallestZ.Post.Post != null)
							{
								smallestZ.Pre.Post = smallestZ.Post.Post;
								smallestZ.Post.Post.Pre = smallestZ.Pre;
							}
							else
							{
								smallestZ.Pre.Post = null;
							}
						}
						else
						{
							smallestZ.CumZ = smallestZ.CumZ + cboxz;
						}
					}
					else if (smallestZ.Pre.CumX < px - smallestZ.CumX)
					{
						if (smallestZ.CumZ + cboxz == smallestZ.Pre.CumZ)
						{
							smallestZ.CumX = smallestZ.CumX - cboxx;
							itemsToPack[cboxi].CoordLength = smallestZ.CumX - cboxx;
						}
						else
						{
							itemsToPack[cboxi].CoordLength = smallestZ.Pre.CumX;
							smallestZ.Pre.Post = new ScrapPad();

							smallestZ.Pre.Post.Pre = smallestZ.Pre;
							smallestZ.Pre.Post.Post = smallestZ;
							smallestZ.Pre = smallestZ.Pre.Post;
							smallestZ.Pre.CumX = smallestZ.Pre.Pre.CumX + cboxx;
							smallestZ.Pre.CumZ = smallestZ.CumZ + cboxz;
						}
					}
					else
					{
						if (smallestZ.CumZ + cboxz == smallestZ.Pre.CumZ)
						{
							smallestZ.Pre.CumX = smallestZ.Pre.CumX + cboxx;
							itemsToPack[cboxi].CoordLength = smallestZ.Pre.CumX;
						}
						else
						{
							itemsToPack[cboxi].CoordLength = smallestZ.CumX - cboxx;
							smallestZ.Post.Pre = new ScrapPad();

							smallestZ.Post.Pre.Post = smallestZ.Post;
							smallestZ.Post.Pre.Pre = smallestZ;
							smallestZ.Post = smallestZ.Post.Pre;
							smallestZ.Post.CumX = smallestZ.CumX;
							smallestZ.Post.CumZ = smallestZ.CumZ + cboxz;
							smallestZ.CumX = smallestZ.CumX - cboxx;
						}
					}

					VolumeCheck();
				}
				else
				{
					//*** SUBSITUATION-4B: SIDES ARE NOT EQUAL TO EACH OTHER ***

					lenx = smallestZ.CumX - smallestZ.Pre.CumX;
					lenz = smallestZ.Pre.CumZ - smallestZ.CumZ;
					lpz = remainpz - smallestZ.CumZ;
					FindBox(lenx, layerThickness, remainpy, lenz, lpz);
					CheckFound();

					if (layerDone) break;
					if (evened) continue;

					itemsToPack[cboxi].CoordWidth = packedy;
					itemsToPack[cboxi].CoordHeight = smallestZ.CumZ;
					itemsToPack[cboxi].CoordLength = smallestZ.Pre.CumX;

					if (cboxx == (smallestZ.CumX - smallestZ.Pre.CumX))
					{
						if ((smallestZ.CumZ + cboxz) == smallestZ.Pre.CumZ)
						{
							smallestZ.Pre.CumX = smallestZ.CumX;
							smallestZ.Pre.Post = smallestZ.Post;
							smallestZ.Post.Pre = smallestZ.Pre;
						}
						else
						{
							smallestZ.CumZ = smallestZ.CumZ + cboxz;
						}
					}
					else
					{
						if ((smallestZ.CumZ + cboxz) == smallestZ.Pre.CumZ)
						{
							smallestZ.Pre.CumX = smallestZ.Pre.CumX + cboxx;
						}
						else if (smallestZ.CumZ + cboxz == smallestZ.Post.CumZ)
						{
							itemsToPack[cboxi].CoordLength = smallestZ.CumX - cboxx;
							smallestZ.CumX = smallestZ.CumX - cboxx;
						}
						else
						{
							smallestZ.Pre.Post = new ScrapPad();

							smallestZ.Pre.Post.Pre = smallestZ.Pre;
							smallestZ.Pre.Post.Post = smallestZ;
							smallestZ.Pre = smallestZ.Pre.Post;
							smallestZ.Pre.CumX = smallestZ.Pre.Pre.CumX + cboxx;
							smallestZ.Pre.CumZ = smallestZ.CumZ + cboxz;
						}
					}

					VolumeCheck();
				}
			}
		}

		/// <summary>
		/// Prints the specified list of things to the console.
		/// </summary>
		private void Print(params dynamic[] list)
		{
			string output = string.Empty;

			for (int i = 0; i < list.Length; i++)
			{
				output += list[i] + " ";
			}

			Console.WriteLine(output);
		}

		/// <summary>
		/// Using the parameters found, packs the best solution found and
		/// reports to the console.
		/// </summary>
		private void Report()
		{
			quit = false;

			switch (bestVariant)
			{
				case 1:
					px = container.Length; py = container.Height; pz = container.Width;
					break;

				case 2:
					px = container.Width; py = container.Height; pz = container.Length;
					break;

				case 3:
					px = container.Width; py = container.Length; pz = container.Height;
					break;

				case 4:
					px = container.Height; py = container.Length; pz = container.Width;
					break;

				case 5:
					px = container.Length; py = container.Width; pz = container.Height;
					break;

				case 6:
					px = container.Height; py = container.Width; pz = container.Length;
					break;
			}

			packingBest = true;

			percentagePackedItemsByVolume = bestVolume * 100 / totalItemVolume;
			percentageContainerUsed = bestVolume * 100 / totalContainerVolume;

			Print("TOTAL NUMBER OF ITERATIONS DONE                       :", iterationsCount);
			Print("BEST SOLUTION FOUND AT ITERATION                      :", bestIteration, "OF VARIANT", bestVariant);
			Print("TOTAL ITEMS TO PACK                                   :", itemsToPackCount);
			Print("PACKED ITEM COUNT                                     :", bestPackedItemCount);
			Print("TOTAL VOLUME OF ALL ITEMS                             :", totalItemVolume);
			Print("CONTAINER VOLUME                                      :", totalContainerVolume);
			Print("BEST SOLUTION'S VOLUME UTILIZATION                    :", bestVolume, "OUT OF", totalContainerVolume);
			Print("PERCENTAGE OF CONTAINER VOLUME USED                   :", percentageContainerUsed);
			Print("PERCENTAGE OF PACKED ITEMS (VOLUME)                   :", percentagePackedItemsByVolume);
			Print("WHILE CONTAINER ORIENTATION X - Y - Z                 :", px, py, pz);
			Print("---------------------------------------------------------------------------------------------");
			Print("  NO: PACKSTA DIMEN-1  DMEN-2  DIMEN-3   COOR-X   COOR-Y   COOR-Z   PACKEDX  PACKEDY  PACKEDZ");
			Print("---------------------------------------------------------------------------------------------");

			layers.Clear();
			layers.Add(new Layer { LayerEval = -1 });
			ListCanditLayers();
			layers = layers.OrderBy(l => l.LayerEval).ToList();
			packedVolume = 0;
			packedy = 0;
			packing = true;
			layerThickness = layers[bestIteration].LayerDim;
			remainpy = py;
			remainpz = pz;

			for (x = 1; x <= itemsToPackCount; x++)
			{
				itemsToPack[x].IsPacked = false;
			}

			do
			{
				layerinlayer = 0;
				layerDone = false;
				PackLayer();
				packedy = packedy + layerThickness;
				remainpy = py - packedy;

				if (layerinlayer > 0.0001M)
				{
					prepackedy = packedy;
					preremainpy = remainpy;
					remainpy = layerThickness - prelayer;
					packedy = packedy - layerThickness + prelayer;
					remainpz = lilz;
					layerThickness = layerinlayer;
					layerDone = false;
					PackLayer();
					packedy = prepackedy;
					remainpy = preremainpy;
					remainpz = pz;
				}

				if (!quit)
				{
					FindLayer(remainpy);
				}
			} while (packing && !quit);

			Console.WriteLine();
			Console.WriteLine();
			Print("*** LIST OF UNPACKED BOXES ***");
			unpacked = true;

			for (cboxi = 1; cboxi <= itemsToPackCount; cboxi++)
			{
				if (!itemsToPack[cboxi].IsPacked)
				{
					GraphUnpackedOut();
				}
			}

			unpacked = false;

			Console.WriteLine();

			for (n = 1; n <= itemsToPackCount; n++)
			{
				if (itemsToPack[n].IsPacked)
				{
					Print(n, itemsToPack[n].Length, itemsToPack[n].Width, itemsToPack[n].Height, itemsToPack[n].CoordLength, itemsToPack[n].CoordWidth, itemsToPack[n].CoordHeight, itemsToPack[n].PackLength, itemsToPack[n].PackWidth, itemsToPack[n].PackHeight);
				}
			}

			Print("TOTAL NUMBER OF ITERATIONS DONE    : ", iterationsCount);
			Print("BEST SOLUTION FOUND AT             : ITERATION: " + bestIteration + " OF VARIANT: ", bestVariant);
			Print("TOTAL NUMBER OF BOXES              : ", itemsToPackCount);
			Print("PACKED NUMBER OF BOXES             : ", bestPackedItemCount);
			Print("TOTAL VOLUME OF ALL BOXES          : ", totalItemVolume);
			Print("PALLET VOLUME                      : ", totalContainerVolume);
			Print("BEST SOLUTION'S VOLUME UTILIZATION : " + bestVolume + " OUT OF ", bestVolume, totalContainerVolume);
			Print("PERCENTAGE OF PALLET VOLUME USED   : ", percentageContainerUsed);
			Print("PERCENTAGE OF PACKEDBOXES (VOLUME) : ", percentagePackedItemsByVolume);
			Print("WHILE PALLET ORIENTATION           : X = " + px + "; Y = " + py + "; Z = " + pz);
		}

		/// <summary>
		/// After packing of each item, the 100% packing condition is checked.
		/// </summary>
		private void VolumeCheck()
		{
			itemsToPack[cboxi].IsPacked = true;
			itemsToPack[cboxi].PackLength = cboxx;
			itemsToPack[cboxi].PackWidth = cboxy;
			itemsToPack[cboxi].PackHeight = cboxz;
			packedVolume = packedVolume + itemsToPack[cboxi].Volume;
			packedItemCount++;

			if (packingBest)
			{
				GraphUnpackedOut();
				OutputBoxList();
			}
			else if (packedVolume == totalContainerVolume || packedVolume == totalItemVolume)
			{
				packing = false;
				hundredPercentPacked = true;
			}
		}

		#endregion Private Methods

		#region Private Classes

		/// <summary>
		/// A list that stores all the different lengths of all item dimensions.
		/// From the master's thesis:
		/// "Each Layerdim value in this array represents a different layer thickness
		/// value with which each iteration can start packing. Before starting iterations,
		/// all different lengths of all box dimensions along with evaluation values are
		/// stored in this array" (p. 3-6).
		/// </summary>
		private class Layer
		{
			/// <summary>
			/// Gets or sets the layer dimension value, representing a layer thickness.
			/// </summary>
			/// <value>
			/// The layer dimension value.
			/// </value>
			public decimal LayerDim { get; set; }

			/// <summary>
			/// Gets or sets the layer eval value, representing an evaluation weight
			/// value for the corresponding LayerDim value.
			/// </summary>
			/// <value>
			/// The layer eval value.
			/// </value>
			public decimal LayerEval { get; set; }
		}

		/// <summary>
		/// From the master's thesis:
		/// "The double linked list we use keeps the topology of the edge of the 
		/// current layer under construction. We keep the x and z coordinates of 
		/// each gap's right corner. The program looks at those gaps and tries to 
		/// fill them with boxes one at a time while trying to keep the edge of the
		/// layer even" (p. 3-7).
		/// </summary>
		private class ScrapPad
		{
			/// <summary>
			/// Gets or sets the x coordinate of the gap's right corner.
			/// </summary>
			/// <value>
			/// The x coordinate of the gap's right corner.
			/// </value>
			public decimal CumX { get; set; }

			/// <summary>
			/// Gets or sets the z coordinate of the gap's right corner.
			/// </summary>
			/// <value>
			/// The z coordinate of the gap's right corner.
			/// </value>
			public decimal CumZ { get; set; }

			/// <summary>
			/// Gets or sets the following entry.
			/// </summary>
			/// <value>
			/// The following entry.
			/// </value>
			public ScrapPad Post { get; set; }

			/// <summary>
			/// Gets or sets the previous entry.
			/// </summary>
			/// <value>
			/// The previous entry.
			/// </value>
			public ScrapPad Pre { get; set; }
		}

		#endregion Private Classes
	}
}