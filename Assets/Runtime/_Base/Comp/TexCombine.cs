using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ProjectX
{
    public class RectangularPacking
    {
        public enum Heuristic
        {
            BestShortSideFit,   // -BSSF: Positions the rectangle against the short side of a free rectangle into which it fits the best.
            BestLongSideFit,    // -BLSF: Positions the rectangle against the long side of a free rectangle into which it fits the best.
            BestAreaFit,        // -BAF: Positions the rectangle into the smallest free rect into which it fits.
            BottomLeftRule,     // -BL: Does the Tetris placement.
            ContactPointRule    // -CP: Choosest the placement where the rectangle touches other rects as much as possible.
        };

        private int mBinWidth = 0;
        private int mBinHeight = 0;
        private bool mAllowRotations;

        private List<Rect> mUsedRects = new List<Rect>();
        private List<Rect> mFreeRects = new List<Rect>();

        public RectangularPacking(int width, int height, bool rotations)
        {
            this.Reset(width, height, rotations);
        }

        public void Reset(int width, int height, bool rotations)
        {
            mBinWidth = width;
            mBinHeight = height;
            mAllowRotations = rotations;

            mUsedRects.Clear();
            mFreeRects.Clear();

            Rect n = default(Rect);
            n.Set(0, 0, width, height);
            mFreeRects.Add(n);
        }

        public Rect Insert(int width, int height, Heuristic method)
        {
            Rect newNode = default(Rect);
            int score1 = 0; // Unused in this function. We don't need to know the score after finding the position.
            int score2 = 0;
            switch (method)
            {
                case Heuristic.BestShortSideFit:
                    newNode = this.FindPositionForNewNodeBestShortSideFit(width, height, ref score1, ref score2);
                    break;
                case Heuristic.BottomLeftRule:
                    newNode = this.FindPositionForNewNodeBottomLeft(width, height, ref score1, ref score2);
                    break;
                case Heuristic.ContactPointRule:
                    newNode = this.FindPositionForNewNodeContactPoint(width, height, ref score1);
                    break;
                case Heuristic.BestLongSideFit:
                    newNode = this.FindPositionForNewNodeBestLongSideFit(width, height, ref score2, ref score1);
                    break;
                case Heuristic.BestAreaFit:
                    newNode = this.FindPositionForNewNodeBestAreaFit(width, height, ref score1, ref score2);
                    break;
            }

            if (newNode.height == 0)
                return newNode;

            int numRectanglesToProcess = mFreeRects.Count;
            for (int i = 0; i < numRectanglesToProcess; ++i)
            {
                if (SplitFreeNode(mFreeRects[i], ref newNode))
                {
                    mFreeRects.RemoveAt(i);
                    --i;
                    --numRectanglesToProcess;
                }
            }

            PruneFreeList();

            mUsedRects.Add(newNode);
            return newNode;
        }

        public void Insert(List<Rect> rects, List<Rect> dst, Heuristic method)
        {
            dst.Clear();

            while (rects.Count > 0)
            {
                int bestScore1 = int.MaxValue;
                int bestScore2 = int.MaxValue;
                int bestRectIndex = -1;
                Rect bestNode = default(Rect);

                for (int i = 0; i < rects.Count; ++i)
                {
                    int score1 = 0;
                    int score2 = 0;
                    Rect newNode = ScoreRect((int)rects[i].width, (int)rects[i].height, method, ref score1, ref score2);

                    if (score1 < bestScore1 || (score1 == bestScore1 && score2 < bestScore2))
                    {
                        bestScore1 = score1;
                        bestScore2 = score2;
                        bestNode = newNode;
                        bestRectIndex = i;
                    }
                }

                if (bestRectIndex == -1)
                    return;

                PlaceRect(bestNode);
                rects.RemoveAt(bestRectIndex);
            }
        }

        void PlaceRect(Rect node)
        {
            int numRectanglesToProcess = mFreeRects.Count;
            for (int i = 0; i < numRectanglesToProcess; ++i)
            {
                if (SplitFreeNode(mFreeRects[i], ref node))
                {
                    mFreeRects.RemoveAt(i);
                    --i;
                    --numRectanglesToProcess;
                }
            }

            PruneFreeList();

            mUsedRects.Add(node);
        }

        Rect ScoreRect(int width, int height, Heuristic method, ref int score1, ref int score2)
        {
            Rect newNode = default(Rect);
            score1 = int.MaxValue;
            score2 = int.MaxValue;
            switch (method)
            {
                case Heuristic.BestShortSideFit:
                    newNode = FindPositionForNewNodeBestShortSideFit(width, height, ref score1, ref score2);
                    break;
                case Heuristic.BottomLeftRule:
                    newNode = FindPositionForNewNodeBottomLeft(width, height, ref score1, ref score2);
                    break;
                case Heuristic.ContactPointRule:
                    newNode = FindPositionForNewNodeContactPoint(width, height, ref score1);
                    score1 = -score1; // Reverse since we are minimizing, but for contact point score bigger is better.
                    break;
                case Heuristic.BestLongSideFit:
                    newNode = FindPositionForNewNodeBestLongSideFit(width, height, ref score2, ref score1); break;
                case Heuristic.BestAreaFit:
                    newNode = FindPositionForNewNodeBestAreaFit(width, height, ref score1, ref score2);
                    break;
            }

            // Cannot fit the current rectangle.
            if (newNode.height == 0)
            {
                score1 = int.MaxValue;
                score2 = int.MaxValue;
            }

            return newNode;
        }

        /// Computes the ratio of used surface area.
        public float Occupancy()
        {
            ulong usedSurfaceArea = 0;
            for (int i = 0; i < mUsedRects.Count; ++i)
                usedSurfaceArea += (uint)mUsedRects[i].width * (uint)mUsedRects[i].height;

            return (float)usedSurfaceArea / (mBinWidth * mBinHeight);
        }

        Rect FindPositionForNewNodeBottomLeft(int width, int height, ref int bestY, ref int bestX)
        {
            Rect bestNode = default(Rect);

            bestY = int.MaxValue;

            for (int i = 0; i < mFreeRects.Count; ++i)
            {
                // Try to place the rectangle in upright (non-flipped) orientation.
                if (mFreeRects[i].width >= width && mFreeRects[i].height >= height)
                {
                    int topSideY = (int)mFreeRects[i].y + height;
                    if (topSideY < bestY || (topSideY == bestY && mFreeRects[i].x < bestX))
                    {
                        bestNode.x = mFreeRects[i].x;
                        bestNode.y = mFreeRects[i].y;
                        bestNode.width = width;
                        bestNode.height = height;
                        bestY = topSideY;
                        bestX = (int)mFreeRects[i].x;
                    }
                }
                if (mAllowRotations && mFreeRects[i].width >= height && mFreeRects[i].height >= width)
                {
                    int topSideY = (int)mFreeRects[i].y + width;
                    if (topSideY < bestY || (topSideY == bestY && mFreeRects[i].x < bestX))
                    {
                        bestNode.x = mFreeRects[i].x;
                        bestNode.y = mFreeRects[i].y;
                        bestNode.width = height;
                        bestNode.height = width;
                        bestY = topSideY;
                        bestX = (int)mFreeRects[i].x;
                    }
                }
            }
            return bestNode;
        }

        Rect FindPositionForNewNodeBestShortSideFit(int width, int height, ref int bestShortSideFit, ref int bestLongSideFit)
        {
            Rect bestNode = default(Rect);

            bestShortSideFit = int.MaxValue;

            for (int i = 0; i < mFreeRects.Count; ++i)
            {
                // Try to place the rectangle in upright (non-flipped) orientation.
                if (mFreeRects[i].width >= width && mFreeRects[i].height >= height)
                {
                    int leftoverHoriz = Mathf.Abs((int)mFreeRects[i].width - width);
                    int leftoverVert = Mathf.Abs((int)mFreeRects[i].height - height);
                    int shortSideFit = Mathf.Min(leftoverHoriz, leftoverVert);
                    int longSideFit = Mathf.Max(leftoverHoriz, leftoverVert);

                    if (shortSideFit < bestShortSideFit || (shortSideFit == bestShortSideFit && longSideFit < bestLongSideFit))
                    {
                        bestNode.x = mFreeRects[i].x;
                        bestNode.y = mFreeRects[i].y;
                        bestNode.width = width;
                        bestNode.height = height;
                        bestShortSideFit = shortSideFit;
                        bestLongSideFit = longSideFit;
                    }
                }

                if (mAllowRotations && mFreeRects[i].width >= height && mFreeRects[i].height >= width)
                {
                    int flippedLeftoverHoriz = Mathf.Abs((int)mFreeRects[i].width - height);
                    int flippedLeftoverVert = Mathf.Abs((int)mFreeRects[i].height - width);
                    int flippedShortSideFit = Mathf.Min(flippedLeftoverHoriz, flippedLeftoverVert);
                    int flippedLongSideFit = Mathf.Max(flippedLeftoverHoriz, flippedLeftoverVert);

                    if (flippedShortSideFit < bestShortSideFit || (flippedShortSideFit == bestShortSideFit && flippedLongSideFit < bestLongSideFit))
                    {
                        bestNode.x = mFreeRects[i].x;
                        bestNode.y = mFreeRects[i].y;
                        bestNode.width = height;
                        bestNode.height = width;
                        bestShortSideFit = flippedShortSideFit;
                        bestLongSideFit = flippedLongSideFit;
                    }
                }
            }
            return bestNode;
        }

        Rect FindPositionForNewNodeBestLongSideFit(int width, int height, ref int bestShortSideFit, ref int bestLongSideFit)
        {
            Rect bestNode = default(Rect);

            bestLongSideFit = int.MaxValue;

            for (int i = 0; i < mFreeRects.Count; ++i)
            {
                // Try to place the rectangle in upright (non-flipped) orientation.
                if (mFreeRects[i].width >= width && mFreeRects[i].height >= height)
                {
                    int leftoverHoriz = Mathf.Abs((int)mFreeRects[i].width - width);
                    int leftoverVert = Mathf.Abs((int)mFreeRects[i].height - height);
                    int shortSideFit = Mathf.Min(leftoverHoriz, leftoverVert);
                    int longSideFit = Mathf.Max(leftoverHoriz, leftoverVert);

                    if (longSideFit < bestLongSideFit || (longSideFit == bestLongSideFit && shortSideFit < bestShortSideFit))
                    {
                        bestNode.x = mFreeRects[i].x;
                        bestNode.y = mFreeRects[i].y;
                        bestNode.width = width;
                        bestNode.height = height;
                        bestShortSideFit = shortSideFit;
                        bestLongSideFit = longSideFit;
                    }
                }

                if (mAllowRotations && mFreeRects[i].width >= height && mFreeRects[i].height >= width)
                {
                    int leftoverHoriz = Mathf.Abs((int)mFreeRects[i].width - height);
                    int leftoverVert = Mathf.Abs((int)mFreeRects[i].height - width);
                    int shortSideFit = Mathf.Min(leftoverHoriz, leftoverVert);
                    int longSideFit = Mathf.Max(leftoverHoriz, leftoverVert);

                    if (longSideFit < bestLongSideFit || (longSideFit == bestLongSideFit && shortSideFit < bestShortSideFit))
                    {
                        bestNode.x = mFreeRects[i].x;
                        bestNode.y = mFreeRects[i].y;
                        bestNode.width = height;
                        bestNode.height = width;
                        bestShortSideFit = shortSideFit;
                        bestLongSideFit = longSideFit;
                    }
                }
            }
            return bestNode;
        }

        Rect FindPositionForNewNodeBestAreaFit(int width, int height, ref int bestAreaFit, ref int bestShortSideFit)
        {
            Rect bestNode = default(Rect);

            bestAreaFit = int.MaxValue;

            for (int i = 0; i < mFreeRects.Count; ++i)
            {
                int areaFit = (int)mFreeRects[i].width * (int)mFreeRects[i].height - width * height;

                // Try to place the rectangle in upright (non-flipped) orientation.
                if (mFreeRects[i].width >= width && mFreeRects[i].height >= height)
                {
                    int leftoverHoriz = Mathf.Abs((int)mFreeRects[i].width - width);
                    int leftoverVert = Mathf.Abs((int)mFreeRects[i].height - height);
                    int shortSideFit = Mathf.Min(leftoverHoriz, leftoverVert);

                    if (areaFit < bestAreaFit || (areaFit == bestAreaFit && shortSideFit < bestShortSideFit))
                    {
                        bestNode.x = mFreeRects[i].x;
                        bestNode.y = mFreeRects[i].y;
                        bestNode.width = width;
                        bestNode.height = height;
                        bestShortSideFit = shortSideFit;
                        bestAreaFit = areaFit;
                    }
                }

                if (mAllowRotations && mFreeRects[i].width >= height && mFreeRects[i].height >= width)
                {
                    int leftoverHoriz = Mathf.Abs((int)mFreeRects[i].width - height);
                    int leftoverVert = Mathf.Abs((int)mFreeRects[i].height - width);
                    int shortSideFit = Mathf.Min(leftoverHoriz, leftoverVert);

                    if (areaFit < bestAreaFit || (areaFit == bestAreaFit && shortSideFit < bestShortSideFit))
                    {
                        bestNode.x = mFreeRects[i].x;
                        bestNode.y = mFreeRects[i].y;
                        bestNode.width = height;
                        bestNode.height = width;
                        bestShortSideFit = shortSideFit;
                        bestAreaFit = areaFit;
                    }
                }
            }
            return bestNode;
        }

        /// Returns 0 if the two intervals i1 and i2 are disjoint, or the length of their overlap otherwise.
        int CommonIntervalLength(int i1start, int i1end, int i2start, int i2end)
        {
            if (i1end < i2start || i2end < i1start)
                return 0;
            return Mathf.Min(i1end, i2end) - Mathf.Max(i1start, i2start);
        }

        int ContactPointScoreNode(int x, int y, int width, int height)
        {
            int score = 0;

            if (x == 0 || x + width == mBinWidth)
                score += height;
            if (y == 0 || y + height == mBinHeight)
                score += width;

            for (int i = 0; i < mUsedRects.Count; ++i)
            {
                if (mUsedRects[i].x == x + width || mUsedRects[i].x + mUsedRects[i].width == x)
                    score += CommonIntervalLength((int)mUsedRects[i].y, (int)mUsedRects[i].y + (int)mUsedRects[i].height, y, y + height);
                if (mUsedRects[i].y == y + height || mUsedRects[i].y + mUsedRects[i].height == y)
                    score += CommonIntervalLength((int)mUsedRects[i].x, (int)mUsedRects[i].x + (int)mUsedRects[i].width, x, x + width);
            }
            return score;
        }

        Rect FindPositionForNewNodeContactPoint(int width, int height, ref int bestContactScore)
        {
            Rect bestNode = default(Rect);

            bestContactScore = -1;

            for (int i = 0; i < mFreeRects.Count; ++i)
            {
                // Try to place the rectangle in upright (non-flipped) orientation.
                if (mFreeRects[i].width >= width && mFreeRects[i].height >= height)
                {
                    int score = ContactPointScoreNode((int)mFreeRects[i].x, (int)mFreeRects[i].y, width, height);
                    if (score > bestContactScore)
                    {
                        bestNode.x = (int)mFreeRects[i].x;
                        bestNode.y = (int)mFreeRects[i].y;
                        bestNode.width = width;
                        bestNode.height = height;
                        bestContactScore = score;
                    }
                }
                if (mAllowRotations && mFreeRects[i].width >= height && mFreeRects[i].height >= width)
                {
                    int score = ContactPointScoreNode((int)mFreeRects[i].x, (int)mFreeRects[i].y, height, width);
                    if (score > bestContactScore)
                    {
                        bestNode.x = (int)mFreeRects[i].x;
                        bestNode.y = (int)mFreeRects[i].y;
                        bestNode.width = height;
                        bestNode.height = width;
                        bestContactScore = score;
                    }
                }
            }
            return bestNode;
        }

        bool SplitFreeNode(Rect freeNode, ref Rect usedNode)
        {
            // Test with SAT if the rectangles even intersect.
            if (usedNode.x >= freeNode.x + freeNode.width || usedNode.x + usedNode.width <= freeNode.x ||
                usedNode.y >= freeNode.y + freeNode.height || usedNode.y + usedNode.height <= freeNode.y)
                return false;

            if (usedNode.x < freeNode.x + freeNode.width && usedNode.x + usedNode.width > freeNode.x)
            {
                // New node at the top side of the used node.
                if (usedNode.y > freeNode.y && usedNode.y < freeNode.y + freeNode.height)
                {
                    Rect newNode = freeNode;
                    newNode.height = usedNode.y - newNode.y;
                    mFreeRects.Add(newNode);
                }

                // New node at the bottom side of the used node.
                if (usedNode.y + usedNode.height < freeNode.y + freeNode.height)
                {
                    Rect newNode = freeNode;
                    newNode.y = usedNode.y + usedNode.height;
                    newNode.height = freeNode.y + freeNode.height - (usedNode.y + usedNode.height);
                    mFreeRects.Add(newNode);
                }
            }

            if (usedNode.y < freeNode.y + freeNode.height && usedNode.y + usedNode.height > freeNode.y)
            {
                // New node at the left side of the used node.
                if (usedNode.x > freeNode.x && usedNode.x < freeNode.x + freeNode.width)
                {
                    Rect newNode = freeNode;
                    newNode.width = usedNode.x - newNode.x;
                    mFreeRects.Add(newNode);
                }

                // New node at the right side of the used node.
                if (usedNode.x + usedNode.width < freeNode.x + freeNode.width)
                {
                    Rect newNode = freeNode;
                    newNode.x = usedNode.x + usedNode.width;
                    newNode.width = freeNode.x + freeNode.width - (usedNode.x + usedNode.width);
                    mFreeRects.Add(newNode);
                }
            }

            return true;
        }

        void PruneFreeList()
        {
            for (int i = 0; i < mFreeRects.Count; ++i)
            {
                for (int j = i + 1; j < mFreeRects.Count; ++j)
                {
                    if (this.IsContainedIn(mFreeRects[i], mFreeRects[j]))
                    {
                        mFreeRects.RemoveAt(i);
                        --i;
                        break;
                    }
                    if (this.IsContainedIn(mFreeRects[j], mFreeRects[i]))
                    {
                        mFreeRects.RemoveAt(j);
                        --j;
                    }
                }
            }
        }

        bool IsContainedIn(Rect a, Rect b)
        {
            return a.x >= b.x && a.y >= b.y
                && a.x + a.width <= b.x + b.width
                && a.y + a.height <= b.y + b.height;
        }
    }

    public class TexCombine
    {
        private struct PackingData
        {
            public Rect rect;
            public bool paddingX;
            public bool paddingY;
        }
        private class PackingResult
        {
            public RectangularPacking packing;
            public int width;
            public int height;
            public PackingData[] data;
        }

        public static Rect[] Combine(Texture2D texture, Texture2D[] textures, int width, int height, bool forceSquare, int padding, int maxSize)
        {
            PackingResult result = new PackingResult();
            if (!TexCombine.Packing(ref result, textures, width, height, forceSquare, padding, maxSize))
                return null;

            width = result.width;
            height = result.height;

            texture.Resize(width, height);
            texture.SetPixels(new Color[width * height]);

            // The returned rects
            Rect[] rects = new Rect[textures.Length];

            for (int i = 0; i < textures.Length; i++)
            {
                Texture2D tex = textures[i];
                if (tex == null)
                    continue;

                Rect rect = result.data[i].rect;
                int xPadding = (result.data[i].paddingX ? padding : 0);
                int yPadding = (result.data[i].paddingY ? padding : 0);
                Color[] colors = tex.GetPixels();

                // Would be used to rotate the texture if need be.
                if (rect.width != tex.width + xPadding)
                {
                    Color[] newColors = tex.GetPixels();

                    for (int x = 0; x < rect.width; x++)
                    {
                        for (int y = 0; y < rect.height; y++)
                        {
                            int prevIndex = ((int)rect.height - (y + 1)) + x * (int)tex.width;
                            newColors[x + y * (int)rect.width] = colors[prevIndex];
                        }
                    }

                    colors = newColors;
                }

                texture.SetPixels((int)rect.x, (int)rect.y, (int)rect.width - xPadding, (int)rect.height - yPadding, colors);
                rect.x /= width;
                rect.y /= height;
                rect.width = (rect.width - xPadding) / width;
                rect.height = (rect.height - yPadding) / height;
                rects[i] = rect;
            }
            texture.Apply();
            return rects;
        }

        private static bool Packing(ref PackingResult result, Texture2D[] textures, int width, int height, bool forceSquare, int padding, int maxSize)
        {
            if (width > maxSize && height > maxSize)
                return false;

            if (width > maxSize || height > maxSize)
            {
                int temp = width;
                width = height;
                height = temp;
            }
            if (forceSquare)
            {
                if (width > height)
                    height = width;
                else if (height > width)
                    width = height;
            }

            if (result.packing == null)
                result.packing = new RectangularPacking(width, height, false);
            else
                result.packing.Reset(width, height, false);

            result.width = width;
            result.height = height;

            if (result.data == null)
                result.data = new PackingData[textures.Length];

            for (int i = 0; i < textures.Length; i++)
            {
                Texture2D tex = textures[i];
                if (tex == null)
                    continue;

                Rect rect = default(Rect);

                int xPadding = 1;
                int yPadding = 1;

                for (xPadding = 1; xPadding >= 0; --xPadding)
                {
                    for (yPadding = 1; yPadding >= 0; --yPadding)
                    {
                        rect = result.packing.Insert(tex.width + (xPadding * padding), tex.height + (yPadding * padding), RectangularPacking.Heuristic.BestAreaFit);
                        if (rect.width != 0 && rect.height != 0)
                            break;

                        // After having no padding if it still doesn't fit -- increase texture size.
                        else if (xPadding == 0 && yPadding == 0)
                        {
                            return TexCombine.Packing(ref result, textures,
                                width * (width <= height ? 2 : 1),
                                height * (height < width ? 2 : 1),
                                forceSquare, padding, maxSize);
                        }
                    }
                    if (rect.width != 0 && rect.height != 0)
                        break;
                }

                result.data[i].rect = rect;
                result.data[i].paddingX = (xPadding != 0);
                result.data[i].paddingY = (yPadding != 0);
            }

            return true;
        }
    }
}
