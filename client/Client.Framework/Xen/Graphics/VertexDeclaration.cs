using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Xen.Graphics
{
    /// <summary>
    /// Attribute for declaring exact use for an element in a vertex
    /// </summary>
    /// <remarks>
    /// <para>The vertex buffer generic class Vertices&lt;T&gt; class will automatically try and determine how the vertex structures fields should be used. However, if the autodetection fails, you can manually specify it with this attirubte.
    /// </para>
    /// <para>Most specical types in <see cref="Microsoft.Xna.Framework.Graphics.PackedVector"/> are automatically supported, so do not need manual declaration.</para>
    /// </remarks>
    /// <example>
    /// <code lang="csharp">
    /// // an example vertex structure
    /// struct VertexWithColour
    /// {
    ///		public Vector3 position;	//will be automatically detected, based on name and type
    ///
    ///		public Color colour0;		//will also be automatically detected as Color type on index 0
    ///
    ///		//public int c1;				//the vertex buffer will fail to automatically detect how to use this value
    ///
    ///		//must manually define it's use:
    ///		//specify Colour, byte4 with use index '1'
    ///
    ///		[VertexElement(VertexElementUsage.Color,VertexElementFormat.Byte4,1)]
    ///		public int c1;
    /// }
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
#if !DEBUG_API
    [System.Diagnostics.DebuggerStepThrough]
#endif
    public sealed class VertexElementAttribute : Attribute
    {
        /// <summary></summary>
        [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
        public sealed class IgnoredAttribute : Attribute { }

        private VertexElementUsage usage;
        private VertexElementFormat? format;
        private byte index;

        /// <summary>
        /// Gets the size (in bytes) of a vertex element format
        /// </summary>
        /// <param name="format"></param>
        /// <returns></returns>
        public static int SizeOfFormatType(VertexElementFormat format)
        {
            return VertexDeclarationBuilder.SizeOfFormatType(format);
        }

        /// <summary>
        /// Computes the stride for a vertex format
        /// </summary>
        /// <param name="elements"></param>
        /// <returns></returns>
        public static int CalculateVertexStride(VertexElement[] elements)
        {
            int stride = 0;
            for (int i = 0; i < elements.Length; i++)
                stride = Math.Max(stride, elements[i].Offset + VertexElementAttribute.SizeOfFormatType(elements[i].VertexElementFormat));
            return stride;
        }

        /// <summary>
        /// Extracts the format and offset of a usage index (simple, but occasionally useful)
        /// </summary>
        /// <param name="elements"></param>
        /// <param name="usage"></param>
        /// <param name="index"></param>
        /// <param name="format"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static bool ExtractUsage(VertexElement[] elements, VertexElementUsage usage, int index, out VertexElementFormat format, out int offset)
        {
            format = (VertexElementFormat)0;
            offset = 0;

            for (int i = 0; i < elements.Length; i++)
            {
                if (elements[i].VertexElementUsage == usage &&
                    elements[i].UsageIndex == index)
                {
                    format = elements[i].VertexElementFormat;
                    offset = elements[i].Offset;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Usage index of the element, eg COLOR1 has a usage index of 1
        /// </summary>
        public byte UsageIndex
        {
            get { return index; }
        }

        /// <summary>
        /// Data type format of the element, eg a <see cref="Vector4"/> is <see cref="VertexElementFormat"/>.Vector4.
        /// </summary>
        public VertexElementFormat? VertexElementFormat
        {
            get { return format; }
        }

        /// <summary>
        /// Usage of the element, eg <see cref="VertexElementUsage"/>.Position.
        /// </summary>
        public VertexElementUsage VertexElementUsage
        {
            get { return usage; }
        }

        /// <summary></summary>
        /// <param name="usage">Usage of the element, eg <see cref="VertexElementUsage"/>.Position.</param>
        /// <param name="format">Data type format of the element, eg a <see cref="Vector4"/> is <see cref="VertexElementFormat"/>.Vector4</param>
        public VertexElementAttribute(VertexElementUsage usage, VertexElementFormat format)
        {
            this.usage = usage;
            this.format = format;
        }

        /// <summary></summary>
        /// <param name="usage">Usage of the element, eg <see cref="VertexElementUsage"/>.Position.</param>
        public VertexElementAttribute(VertexElementUsage usage)
        {
            this.usage = usage;
            this.format = null;
        }

        /// <summary></summary>
        /// <param name="usage">Usage of the element, eg <see cref="VertexElementUsage"/>.Position.</param>
        /// <param name="usageIndex">Usage index of the element, eg COLOR1 has a usage index of 1</param>
        public VertexElementAttribute(VertexElementUsage usage, byte usageIndex)
        {
            this.usage = usage;
            this.format = null;
            this.index = usageIndex;
        }

        /// <summary></summary>
        /// <param name="usage">Usage of the element, eg <see cref="VertexElementUsage"/>.Position.</param>
        /// <param name="usageIndex">Usage index of the element, eg COLOR1 has a usage index of 1</param>
        /// <param name="format">Data type format of the element, eg a <see cref="Vector4"/> is <see cref="VertexElementFormat"/>.Vector4</param>
        public VertexElementAttribute(VertexElementUsage usage, VertexElementFormat format, byte usageIndex)
        {
            this.usage = usage;
            this.format = format;
            this.index = usageIndex;
        }
    }

#if !DEBUG_API

    [System.Diagnostics.DebuggerStepThrough]
#endif
    internal class VertexDeclarationBuilder
    {
#if !DEBUG_API

        [System.Diagnostics.DebuggerStepThrough]
#endif
        private static class Ident
        {
            static object sync = new object();
            static volatile int index = 0;

            private static int Index
            {
                get
                {
                    lock (sync)
                        return index++;
                }
            }

            private class Type<T>
            {
                public static int id = Index;
            }

            public static int TypeIndex<T>()
            {
                return Type<T>.id;
            }
        }

#if DEBUG
        struct VertexUsage
        {
            public VertexElementUsage usage;
            public int index;
        }
#endif

        private static Dictionary<Type, VertexElementFormat> formatMapping;
        private static Dictionary<VertexElementFormat, int> formatMappingSize;
        private static Dictionary<string, VertexElementUsage> usageMapping;
        private Dictionary<Type, VertexElement[]> declarationMapping;
        private Dictionary<Type, short> typeHash;
        private short typeIndex;
        internal static VertexDeclarationBuilder Instance;

        private VertexDeclaration[] declarations = new VertexDeclaration[128];
        private Dictionary<DeclarationHash, VertexDeclaration> declarationHash = new Dictionary<DeclarationHash, VertexDeclaration>();
        private GraphicsDevice creationDevice;
        private Dictionary<ElementHash, VertexDeclaration> elementHash = new Dictionary<ElementHash, VertexDeclaration>();

#if DEBUG
        private Dictionary<VertexDeclaration, VertexUsage[]> vertexDeclarationUsage;
#endif
        private static DeclarationHash hashingDecl;
        private Dictionary<VertexElementFormat, bool> vertexFormatSupported;

#if !DEBUG_API

        [System.Diagnostics.DebuggerStepThrough]
#endif
        private class DeclarationHash : IComparable<DeclarationHash>
        {
            public DeclarationHash(Type type, Dictionary<Type, short> typeHash, ref short typeIndex)
            {
                SetFrom(type, typeHash, ref typeIndex);
            }

            public void SetFrom(Type type, Dictionary<Type, short> typeHash, ref short typeIndex)
            {
                this.hash.Clear();
                short hash;
                this.hashCode = 0;

                lock (typeHash)
                {
                    if (!typeHash.TryGetValue(type, out hash))
                    {
                        hash = typeIndex++;
                        typeHash.Add(type, hash);
                        hashCode ^= hash;
                    }
                }
                this.hash.Add(hash);
            }

            public DeclarationHash(Type[] types, Dictionary<Type, short> typeHash, ref short typeIndex)
            {
                SetFrom(types, typeHash, ref typeIndex);
            }

            public void SetFrom(Type[] types, Dictionary<Type, short> typeHash, ref short typeIndex)
            {
                this.hash.Clear();
                this.hashCode = 0;

                lock (typeHash)
                {
                    for (int i = 0; i < types.Length; i++)
                    {
                        short hash;
                        if (!typeHash.TryGetValue(types[i], out hash))
                        {
                            hash = typeIndex++;
                            typeHash.Add(types[i], hash);
                        }
                        this.hash.Add(hash);
                        hashCode ^= hash;
                    }
                }
            }

            List<short> hash = new List<short>();
            short hashCode;

            public override int GetHashCode()
            {
                return hashCode;
            }

            public int CompareTo(DeclarationHash other)
            {
                int cmp = other.hash.Count.CompareTo(hash.Count);

                if (cmp != 0)
                    return cmp;

                for (int i = 0; i < hash.Count; i++)
                {
                    cmp = hash[i].CompareTo(other.hash[i]);
                    if (cmp != 0)
                        return cmp;
                }
                return 0;
            }

            public override bool Equals(object obj)
            {
                if (obj is IComparable<DeclarationHash>)
                    return ((IComparable<DeclarationHash>)obj).CompareTo(this) == 0;
                return base.Equals(obj);
            }
        }

        private class ElementHash : IComparable<ElementHash>
        {
            int hash;
            VertexElement[] elements;

            public ElementHash(VertexElement[] elements)
            {
                this.elements = elements;
                for (int i = 0; i < elements.Length; i++)
                {
                    hash ^= ((int)elements[i].VertexElementUsage);
                    hash ^= ((int)elements[i].VertexElementFormat) << 6;
                    hash ^= ((int)elements[i].UsageIndex) << 8;
                    hash ^= (int)elements[i].Offset;
                    hash ^= i;
                }
            }

            public override bool Equals(object obj)
            {
                if (obj is ElementHash)
                    return CompareTo(obj as ElementHash) == 0;
                return base.Equals(obj);
            }

            public override int GetHashCode()
            {
                return hash;
            }

            public int CompareTo(ElementHash othervh)
            {
                VertexElement[] other = othervh.elements;

                if (elements.Length != other.Length)
                    return elements.Length - other.Length;

                for (int i = 0; i < elements.Length; i++)
                {
                    if (elements[i].Offset != other[i].Offset)
                        return (int)elements[i].Offset - (int)other[i].Offset;

                    if (elements[i].UsageIndex != other[i].UsageIndex)
                        return (int)elements[i].UsageIndex - (int)other[i].UsageIndex;

                    if (elements[i].VertexElementFormat != other[i].VertexElementFormat)
                        return (int)elements[i].VertexElementFormat - (int)other[i].VertexElementFormat;

                    if (elements[i].VertexElementUsage != other[i].VertexElementUsage)
                        return (int)elements[i].VertexElementUsage - (int)other[i].VertexElementUsage;
                }
                return 0;
            }
        }

        public VertexDeclarationBuilder()
        {
            declarationMapping = new Dictionary<Type, VertexElement[]>();
            typeHash = new Dictionary<Type, short>();
            hashingDecl = new DeclarationHash(typeof(Vector3), typeHash, ref typeIndex);//static to keep from GC messing
            Instance = this;
        }

        static VertexDeclarationBuilder()
        {
            formatMapping = new Dictionary<Type, VertexElementFormat>();
            formatMappingSize = new Dictionary<VertexElementFormat, int>();
            usageMapping = new Dictionary<string, VertexElementUsage>();

            formatMapping.Add(typeof(Microsoft.Xna.Framework.Graphics.PackedVector.Byte4), VertexElementFormat.Byte4);
            formatMapping.Add(typeof(Microsoft.Xna.Framework.Color), VertexElementFormat.Color);
            //formatMapping.Add(typeof(Microsoft.Xna.Framework.Graphics.PackedVector.HalfVector2), VertexElementFormat.HalfVector2);
            //formatMapping.Add(typeof(Microsoft.Xna.Framework.Graphics.PackedVector.HalfVector4), VertexElementFormat.HalfVector4);
            formatMapping.Add(typeof(Microsoft.Xna.Framework.Graphics.PackedVector.NormalizedShort2), VertexElementFormat.NormalizedShort2);
            formatMapping.Add(typeof(Microsoft.Xna.Framework.Graphics.PackedVector.NormalizedShort4), VertexElementFormat.NormalizedShort4);
            formatMapping.Add(typeof(Microsoft.Xna.Framework.Graphics.PackedVector.Short2), VertexElementFormat.Short2);
            formatMapping.Add(typeof(Microsoft.Xna.Framework.Graphics.PackedVector.Short4), VertexElementFormat.Short4);
            formatMapping.Add(typeof(float), VertexElementFormat.Single);
            formatMapping.Add(typeof(Vector2), VertexElementFormat.Vector2);
            formatMapping.Add(typeof(Vector3), VertexElementFormat.Vector3);
            formatMapping.Add(typeof(Vector4), VertexElementFormat.Vector4);

            foreach (KeyValuePair<Type, VertexElementFormat> kvp in formatMapping)
            {
                formatMappingSize.Add(kvp.Value, Marshal.SizeOf(kvp.Key));
            }

            FieldInfo[] enums = typeof(VertexElementUsage).GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);

            foreach (FieldInfo field in enums)
            {
                VertexElementUsage usage = (VertexElementUsage)field.GetValue(null);
                usageMapping.Add(usage.ToString().ToLower(), usage);
            }
            usageMapping.Add("norm", VertexElementUsage.Normal);
            usageMapping.Add("pos", VertexElementUsage.Position);
            usageMapping.Add("binorm", VertexElementUsage.Binormal);
            usageMapping.Add("colour", VertexElementUsage.Color);
            usageMapping.Add("diffuse", VertexElementUsage.Color);
            usageMapping.Add("col", VertexElementUsage.Color);
            usageMapping.Add("size", VertexElementUsage.PointSize);
            usageMapping.Add("psize", VertexElementUsage.PointSize);
            usageMapping.Add("tex", VertexElementUsage.TextureCoordinate);
            usageMapping.Add("texture", VertexElementUsage.TextureCoordinate);
            usageMapping.Add("texcoord", VertexElementUsage.TextureCoordinate);
            usageMapping.Add("texcoordinate", VertexElementUsage.TextureCoordinate);
        }

        internal static int SizeOfFormatType(VertexElementFormat element)
        {
            return formatMappingSize[element];
        }

        private void ValidateDevice(GraphicsDevice device)
        {
            if (creationDevice != device && device != null)
            {
                foreach (VertexDeclaration decl in declarationHash.Values)
                {
                    decl.Dispose();
                }
                foreach (VertexDeclaration decl in elementHash.Values)
                {
                    decl.Dispose();
                }
                elementHash.Clear();
                declarationHash.Clear();
                for (int i = 0; i < declarations.Length; i++)
                    declarations[i] = null;
                creationDevice = device;

                BuildFormatList(device);

#if DEBUG
                if (vertexDeclarationUsage != null)
                    vertexDeclarationUsage.Clear();
#endif
            }
        }

        public VertexDeclaration GetDeclaration<T>(GraphicsDevice device)
        {
            ValidateDevice(device);

            int index = Ident.TypeIndex<T>();

            while (index > this.declarations.Length)
                Array.Resize(ref this.declarations, this.declarations.Length * 2);

            if (this.declarations[index] != null)
                return this.declarations[index];

            VertexElement[] elements = GetDeclaration(typeof(T));

            if (device == null)
                return null;

            VertexDeclaration declaration;
            lock (hashingDecl)
            {
                hashingDecl.SetFrom(typeof(T), typeHash, ref typeIndex);
                if (declarationHash.TryGetValue(hashingDecl, out declaration))
                {
                    this.declarations[index] = declaration;
                    return declaration;
                }
            }

            for (int i = 0; i < elements.Length; i++)
            {
                ValidateFormat(typeof(T), elements[i].VertexElementFormat);
            }

            ElementHash ehash = new ElementHash(elements);
            elementHash.TryGetValue(ehash, out declaration);

            if (declaration == null)
                declaration = new VertexDeclaration(elements);

            this.declarations[index] = declaration;
            declarationHash.Add(new DeclarationHash(typeof(T), typeHash, ref typeIndex), declaration);

            if (elementHash.ContainsKey(ehash) == false)
                elementHash.Add(ehash, declaration);

            return declaration;
        }

        private void BuildFormatList(GraphicsDevice device)
        {
            vertexFormatSupported = new Dictionary<VertexElementFormat, bool>();

            System.Reflection.FieldInfo[] enums = typeof(VertexElementFormat).GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);

            //reach support:
            VertexElementFormat[] reachFormats = { VertexElementFormat.Color, VertexElementFormat.Byte4, VertexElementFormat.Single, VertexElementFormat.Vector2, VertexElementFormat.Vector3, VertexElementFormat.Vector4, VertexElementFormat.Short2, VertexElementFormat.Short4, VertexElementFormat.NormalizedShort2, VertexElementFormat.NormalizedShort4 };

            foreach (var format in reachFormats)
            {
                vertexFormatSupported.Add(format, true);
            }

            //if (device.GraphicsProfile == GraphicsProfile.HiDef)
            //{
            //    vertexFormatSupported.Add(VertexElementFormat.HalfVector2, true);
            //    vertexFormatSupported.Add(VertexElementFormat.HalfVector4, true);
            //}
        }

        public void ValidateArrayDeclaration(GraphicsDevice device, Type[] streamTypes, IVertices[] buffers)
        {
            ValidateDevice(device);

            VertexElement[][] mappings = new VertexElement[streamTypes.Length][];

            int i = 0;
            for (i = 0; i < streamTypes.Length; i++)
            {
                //buffer provides the vertex elements itself
                if (buffers[i] is IDeviceVertexBuffer &&
                    (buffers[i] as IDeviceVertexBuffer).IsImplementationUserSpecifiedVertexElements(out mappings[i]))
                    continue;

                mappings[i] = GetDeclaration(streamTypes[i]);
            }
        }

        public VertexDeclaration GetDeclaration(GraphicsDevice device, VertexElement[] elements)
        {
            ValidateDevice(device);

            VertexDeclaration declaration;
            ElementHash ehash = new ElementHash(elements);

            if (elementHash.TryGetValue(ehash, out declaration))
                return declaration;

            declaration = new VertexDeclaration(elements);
            elementHash.Add(ehash, declaration);

            return declaration;
        }

        public VertexElement[] GetDeclaration(Type type)
        {
            lock (declarationMapping)
            {
                VertexElement[] mapping;
                if (declarationMapping.TryGetValue(type, out mapping))
                    return mapping;

                if (type == typeof(Vector3))//special case, map a single vector to a position element
                    mapping = new VertexElement[] { new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0) };
                if (type == typeof(Vector4))
                    mapping = new VertexElement[] { new VertexElement(0, VertexElementFormat.Vector4, VertexElementUsage.Position, 0) };
                if (type == typeof(Vector2))
                    mapping = new VertexElement[] { new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0) };

                //special case for instancing:
                if (type == typeof(Matrix))
                {
                    mapping = new VertexElement[]
					{
						new VertexElement(0, VertexElementFormat.Vector4, VertexElementUsage.Position, 12),
						new VertexElement(16, VertexElementFormat.Vector4, VertexElementUsage.Position, 13),
						new VertexElement(32, VertexElementFormat.Vector4, VertexElementUsage.Position, 14),
						new VertexElement(48, VertexElementFormat.Vector4, VertexElementUsage.Position, 15),
					};
                }

                if (mapping == null)
                {
                    List<VertexElement> elements = new List<VertexElement>();
                    int offset = 0;

                    if (type.IsValueType == false)
                        throw new ArgumentException("Type " + type.Name + " is a not a ValueType (struct)");

                    foreach (FieldInfo f in type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
                    {
                        if (!f.ReflectedType.IsValueType)
                            throw new ArgumentException("Field " + type.Name + "." + f.Name + " is a not a ValueType (struct)");

                        int size = Marshal.SizeOf(f.FieldType);

                        bool attribSet = false;

                        foreach (object o in f.GetCustomAttributes(true))
                        {
                            if (o is VertexElementAttribute)
                            {
                                VertexElementAttribute att = (VertexElementAttribute)o;
                                VertexElementFormat? format = att.VertexElementFormat;
                                if (format == null)
                                    format = DetermineFormat(f);
                                else
                                {
                                    int formatSize;
                                    if (!formatMappingSize.TryGetValue(format.Value, out formatSize))
                                        throw new ArgumentException(string.Format("Invlaid VertexElementFormat ({0}) specified in VertexElementAttribute for {1}.{2}", format, type.FullName, f.Name));
                                    if (formatSize != Marshal.SizeOf(f.FieldType))
                                        throw new ArgumentException(string.Format("VertexElementFormat size mismatch in {4}.{5}, {0} requires a size of {1}, specified type {2} has size {3}", format, formatSize, f.FieldType.FullName, Marshal.SizeOf(f.FieldType), type.FullName, f.Name));
                                }

                                elements.Add(new VertexElement(offset, format.Value, att.VertexElementUsage, (int)att.UsageIndex));
                                attribSet = true;
                                break;
                            }
                            if (o is VertexElementAttribute.IgnoredAttribute)
                            {
                                attribSet = true;
                                break;
                            }
                        }

                        if (!attribSet)
                        {
                            VertexElementFormat format = DetermineFormat(f);
                            int index;
                            VertexElementUsage usage = DetermineUsage(elements, f, out index);

                            elements.Add(new VertexElement(offset, format, usage, index));
                        }

                        offset += size;
                    }

                    mapping = elements.ToArray();
                }
                declarationMapping.Add(type, mapping);

                return mapping;
            }
        }

        private void ValidateFormat(Type type, VertexElementFormat format)
        {
            bool supported = true;
            if (vertexFormatSupported != null &&
                vertexFormatSupported.TryGetValue(format, out supported) && !supported)
                throw new InvalidOperationException(string.Format("Graphics device does not support vertex element format \'{0}\', as used in vertex structure \'{1}\'", format, type.FullName));
        }

        private static VertexElementFormat DetermineFormat(FieldInfo field)
        {
            VertexElementFormat format;
            if (formatMapping.TryGetValue(field.FieldType, out format))
                return format;
            throw new ArgumentException("Field (" + field.FieldType.Name + ") " + field.DeclaringType.Name + "." + field.Name + " value mapping cannot be determined. Either set the VertexElementFormat with a [VertexElement()] attribute, or change the declaration to a supported type.");
        }

        public static VertexElementFormat DetermineFormat(Type type)
        {
            VertexElementFormat value;
            if (!formatMapping.TryGetValue(type, out value))
                throw new ArgumentException("Unabled to determine vertex format mapping for type '{0}'", type.Name);
            return value;
        }

        private static VertexElementUsage DetermineUsage(List<VertexElement> elements, FieldInfo field, out int index)
        {
            string name = field.Name.ToLower().Replace("_", "");
            string number = "";

            for (int i = name.Length - 1; i >= 0; i--)
            {
                if (char.IsDigit(name[i]))
                    number = name[i] + number;
                else
                    break;
            }
            name = name.Substring(0, name.Length - number.Length);

            index = 0;
            if (number.Length > 0)
                index = int.Parse(number, System.Globalization.CultureInfo.InvariantCulture.NumberFormat);

            VertexElementUsage usage;
            if (usageMapping.TryGetValue(name, out usage))
            {
                while (true)
                {
                    bool valid = true;
                    foreach (VertexElement el in elements)
                    {
                        if (el.VertexElementUsage == usage && el.UsageIndex == index)
                        {
                            index++;
                            valid = false;
                        }
                    }
                    if (valid)
                        break;
                }

                return usage;
            }
            throw new ArgumentException("Field (" + field.FieldType.Name + ") " + field.DeclaringType.Name + "." + field.Name + " usage mapping cannot be determined. Either set the VertexElementUsage with a [VertexElement()] attribute, or change the field name to a known usage type.");
        }

#if DEBUG
        //public void ValidateVertexDeclarationForShader(VertexDeclaration declaration, IShader shader, Type verticesType)
        //{
        //    VertexUsage[] usage;

        //    if (vertexDeclarationUsage == null)
        //        vertexDeclarationUsage = new Dictionary<VertexDeclaration, VertexUsage[]>();

        //    if (!vertexDeclarationUsage.TryGetValue(declaration, out usage))
        //    {
        //        //build usage
        //        VertexElement[] elements = declaration.GetVertexElements();

        //        Dictionary<VertexElementUsage, List<int>> usageIndices = new Dictionary<VertexElementUsage, List<int>>();

        //        foreach (VertexElement ve in elements)
        //        {
        //            List<int> inds;
        //            if (!usageIndices.TryGetValue(ve.VertexElementUsage, out inds))
        //            {
        //                inds = new List<int>();
        //                usageIndices.Add(ve.VertexElementUsage, inds);
        //            }
        //            inds.Add(ve.UsageIndex);
        //        }

        //        //this is a nasty way to get around the lack os SortedList in XNA on the 360
        //        var sortedUsage = new List<VertexElementUsage>(usageIndices.Keys);
        //        sortedUsage.Sort();

        //        List<VertexUsage> usages = new List<VertexUsage>();
        //        foreach (VertexElementUsage usageValue in sortedUsage)
        //        {
        //            var value = usageIndices[usageValue];
        //            VertexUsage vuse = new VertexUsage();
        //            value.Sort();
        //            vuse.usage = usageValue;
        //            foreach (int i in value)
        //            {
        //                vuse.index = i;
        //                usages.Add(vuse);
        //            }
        //        }

        //        usage = usages.ToArray();
        //        vertexDeclarationUsage.Add(declaration, usage);
        //    }
        //    int shaderCount = shader.GetVertexInputCount();

        //    if (shaderCount == 0)
        //        return;

        //    VertexElementUsage use;
        //    int index;
        //    int sv = 0;

        //    for (int dv = 0; dv < usage.Length && sv < shaderCount;)
        //    {
        //        shader.GetVertexInput(sv,out use, out index);

        //        if (usage[dv].usage == use)
        //        {
        //            if (usage[dv].index == index)
        //            {
        //                dv++;//all happy, elements match.
        //                sv++;
        //                continue;
        //            }
        //            if (usage[dv].index > index)
        //            {
        //                //bugger, missing element
        //                break;
        //            }
        //            dv++;
        //            continue;
        //        }
        //        if ((int)use > (int)usage[dv].usage)
        //        {
        //            dv++;
        //            continue;
        //        }
        //        break;//bugger.
        //    }

        //    if (sv < shaderCount)
        //    {
        //        //problems..
        //        shader.GetVertexInput(sv,out use, out index);

        //        //generate an error describing the problem,

        //        //fill it in with details about the state
        //        string vertexType = "VerticesGroup";
        //        string vertexDecl = "vertex structure";
        //        string shaderType = string.Format("type {0}",shader.GetType());

        //        string errorFormat = @"Error: The current vertex shader is attempting to read data that is not present in the vertices being drawn.{5}The shader currently in use ({0}) has a vertex shader that reads '{1}{2}' from each vertex.{5}However, the {3} being drawn does not contain a '{1}{2}' value in it's {4}.";

        //        if (verticesType != null)
        //            vertexType = string.Format("Vertices<{0}> object", verticesType.FullName);

        //        //add some helpers in some common situations...
        //        if (shader.GetType().IsPublic == false &&
        //            shader.GetType().Namespace == "Xen.Ex.Material")
        //        {
        //            shaderType = "MaterialShader";

        //            if (use == VertexElementUsage.Tangent || use == VertexElementUsage.Binormal)
        //            {
        //                errorFormat += Environment.NewLine;
        //                errorFormat += "NOTE: MaterialShader properties may change the vertex data it tries to access. Using a Normal Map requires the vertices have Tangents and Binormals.";
        //            }
        //            if (use == VertexElementUsage.Color)
        //            {
        //                errorFormat += Environment.NewLine;
        //                errorFormat += "NOTE: MaterialShader properties may change the vertex data it tries to access. Setting 'UseVertexColour' to true requires the vertices have Color0 data.";
        //            }
        //            if (use == VertexElementUsage.Normal)
        //            {
        //                errorFormat += Environment.NewLine;
        //                errorFormat += "NOTE: MaterialShader properties may change the vertex data it tries to access. Enabling lighting requires the vertices have Normals.";
        //            }
        //        }

        //        if (verticesType == typeof(byte))
        //        {
        //            vertexType = "XNA vertex data";
        //            vertexDecl = "vertex declaration";

        //            if (use == VertexElementUsage.Tangent || use == VertexElementUsage.Binormal)
        //            {
        //                errorFormat += Environment.NewLine;
        //                errorFormat += "NOTE: If you are drawing a ModelInstance, the Xen Model Importer can generate Tangent/Binormal data by setting the 'Generate Tangent Frames' Content Processor property for the file to true.";
        //            }
        //        }

        //        string error = string.Format(errorFormat, shaderType, use, index, vertexType, vertexDecl, Environment.NewLine);

        //        throw new InvalidOperationException(error);
        //    }
        //}
#endif
    }
}