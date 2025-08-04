using System;
using System.Collections.Generic;

namespace DragonBones
{
    /// <summary>
    /// - Manages bones, slots, constrains of <see cref="DragonBones.Armature"/>
    /// </summary> 
    /// <language>en_US</language>
    public sealed class ArmatureStructure : IDisposable
    {
        public readonly Armature BelongsTo;
        
        public Bone[] Bones => bones.ToArray();
        public Slot[] Slots => slots.ToArray();
        internal Constraint[] Constraints => constraints.ToArray();
        
        private readonly List<Bone> bones = new();
        private readonly List<Slot> slots = new();
        private readonly List<Constraint> constraints = new List<Constraint>();

        public bool SlotsDirty { get; private set; }
        private bool SlotsZOrderDirty;
        public void MarkSlotsAsDirty() => SlotsDirty = true;

        public ArmatureStructure(Armature belongsTo)
        {
            BelongsTo = belongsTo;
        }

        public void Dispose()
        {
            foreach (var bone in bones) { bone.ReturnToPool(); }
            foreach (var slot in slots) { slot.ReturnToPool(); }
            foreach (var constraint in constraints) { constraint.ReturnToPool(); }
            
            bones.Clear();
            slots.Clear();
            constraints.Clear();

            SlotsDirty = false;
        }

        #region Control API
        public void SortSlotsListByZOrder()
        {
            slots.Sort(CompareSlots);
            
            int CompareSlots(Slot a, Slot b)
            {
                if (a._zOrder > b._zOrder)
                {
                    return 1;
                }
                else if (a._zOrder < b._zOrder)
                {
                    return -1;
                }

                return 0;//fixed slots sort error
            }
        }

        internal void SortZOrder(short[] slotIndices, int offset)
        {
            List<SlotData> slotDatas = BelongsTo.ArmatureData.sortedSlots;
            bool isOriginal = slotIndices == null;

            if (SlotsZOrderDirty || !isOriginal)
            {
                for (int i = 0, l = slotDatas.Count; i < l; ++i)
                {
                    var slotIndex = isOriginal ? i : slotIndices[offset + i];
                    if (slotIndex < 0 || slotIndex >= l)
                    {
                        continue;
                    }

                    SlotData slotData = slotDatas[slotIndex];
                    Slot slot = GetSlot(slotData.name);
                    slot?.SetZOrder(i);
                }

                MarkSlotsAsDirty();
                SlotsZOrderDirty = !isOriginal;
            }
        }
        #endregion

        #region Helpers
        /// <summary>
        /// - Check whether a specific point is inside a custom bounding box in a slot.
        /// The coordinate system of the point is the inner coordinate system of the armature.
        /// Custom bounding boxes need to be customized in Dragonbones Pro.
        /// </summary>
        /// <param name="x">- The horizontal coordinate of the point.</param>
        /// <param name="y">- The vertical coordinate of the point.</param>
        /// <version>DragonBones 5.0</version>
        /// <language>en_US</language>
        public Slot GetSlotWithPoint(float x, float y)
        {
            foreach (var slot in Slots)
            {
                if (slot.ContainsPoint(x, y))
                {
                    return slot;
                }
            }

            return null;
        }
        
        /// <summary>
        /// - Check whether a specific segment intersects a custom bounding box for a slot in the armature.
        /// The coordinate system of the segment and intersection is the inner coordinate system of the armature.
        /// Custom bounding boxes need to be customized in Dragonbones Pro.
        /// </summary>
        /// <param name="xA">- The horizontal coordinate of the beginning of the segment.</param>
        /// <param name="yA">- The vertical coordinate of the beginning of the segment.</param>
        /// <param name="xB">- The horizontal coordinate of the end point of the segment.</param>
        /// <param name="yB">- The vertical coordinate of the end point of the segment.</param>
        /// <param name="intersectionPointA">- The first intersection at which a line segment intersects the bounding box from the beginning to the end. (If not set, the intersection point will not calculated)</param>
        /// <param name="intersectionPointB">- The first intersection at which a line segment intersects the bounding box from the end to the beginning. (If not set, the intersection point will not calculated)</param>
        /// <param name="normalRadians">- The normal radians of the tangent of the intersection boundary box. [x: Normal radian of the first intersection tangent, y: Normal radian of the second intersection tangent] (If not set, the normal will not calculated)</param>
        /// <returns>The slot of the first custom bounding box where the segment intersects from the start point to the end point.</returns>
        /// <version>DragonBones 5.0</version>
        /// <language>en_US</language>
        public Slot IntersectsSegment(float xA, float yA, float xB, float yB,
                                       Point intersectionPointA = null,
                                       Point intersectionPointB = null,
                                       Point normalRadians = null)
        {
            var isV = xA == xB;
            var dMin = 0.0f;
            var dMax = 0.0f;
            var intXA = 0.0f;
            var intYA = 0.0f;
            var intXB = 0.0f;
            var intYB = 0.0f;
            var intAN = 0.0f;
            var intBN = 0.0f;
            Slot intSlotA = null;
            Slot intSlotB = null;

            foreach (var slot in Slots)
            {
                var intersectionCount = slot.IntersectsSegment(xA, yA, xB, yB, intersectionPointA, intersectionPointB, normalRadians);
                if (intersectionCount > 0)
                {
                    if (intersectionPointA != null || intersectionPointB != null)
                    {
                        if (intersectionPointA != null)
                        {
                            var d = isV ? intersectionPointA.y - yA : intersectionPointA.x - xA;
                            if (d < 0.0f)
                            {
                                d = -d;
                            }

                            if (intSlotA == null || d < dMin)
                            {
                                dMin = d;
                                intXA = intersectionPointA.x;
                                intYA = intersectionPointA.y;
                                intSlotA = slot;

                                if (normalRadians != null)
                                {
                                    intAN = normalRadians.x;
                                }
                            }
                        }

                        if (intersectionPointB != null)
                        {
                            var d = intersectionPointB.x - xA;
                            if (d < 0.0f)
                            {
                                d = -d;
                            }

                            if (intSlotB == null || d > dMax)
                            {
                                dMax = d;
                                intXB = intersectionPointB.x;
                                intYB = intersectionPointB.y;
                                intSlotB = slot;

                                if (normalRadians != null)
                                {
                                    intBN = normalRadians.y;
                                }
                            }
                        }
                    }
                    else
                    {
                        intSlotA = slot;
                        break;
                    }
                }
            }

            if (intSlotA != null && intersectionPointA != null)
            {
                intersectionPointA.x = intXA;
                intersectionPointA.y = intYA;

                if (normalRadians != null)
                {
                    normalRadians.x = intAN;
                }
            }

            if (intSlotB != null && intersectionPointB != null)
            {
                intersectionPointB.x = intXB;
                intersectionPointB.y = intYB;

                if (normalRadians != null)
                {
                    normalRadians.y = intBN;
                }
            }

            return intSlotA;
        }
        #endregion
        
        #region GetSet API 
        internal void AddBone(Bone value)
        {
            if (!bones.Contains(value))
            {
                bones.Add(value);
            }
        }

        internal void AddSlot(Slot value)
        {
            if (!slots.Contains(value))
            {
                slots.Add(value);
                SlotsDirty = true;
            }
        }

        internal void AddConstraint(Constraint value)
        {
            if (!constraints.Contains(value))
            {
                constraints.Add(value);
            }
        }

        public Bone GetBone(string name)
        {
            foreach (var bone in bones)
            {
                if (bone.name == name)
                {
                    return bone;
                }
            }

            return null;
        }

        public Bone GetBoneByDisplay(object display)
        {
            var slot = GetSlotByDisplay(display);

            return slot != null ? slot.Parent : null;
        }

        public Slot GetSlot(string name)
        {
            foreach (var slot in slots)
            {
                if (slot.name == name)
                {
                    return slot;
                }
            }

            return null;
        }

        public Slot GetSlotByDisplay(object display)
        {
            if (display != null)
            {
                foreach (var slot in slots)
                {
                    if (slot.Display == display)
                    {
                        return slot;
                    }
                }
            }

            return null;
        }
        #endregion
    }
}