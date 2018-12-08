using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Angels.Application.TicketEntity.Common.BuildVRT
{
    using System.Xml.Serialization;


    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class VRTDataset
    {

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("BlockXSize", typeof(string), DataType = "nonNegativeInteger", Order = 0)]
        [System.Xml.Serialization.XmlElementAttribute("BlockYSize", typeof(string), DataType = "nonNegativeInteger", Order = 0)]
        [System.Xml.Serialization.XmlElementAttribute("GCPList", typeof(GCPListType), Order = 0)]
        [System.Xml.Serialization.XmlElementAttribute("GDALWarpOptions", typeof(GDALWarpOptionsType), Order = 0)]
        [System.Xml.Serialization.XmlElementAttribute("GeoTransform", typeof(string), Order = 0)]
        [System.Xml.Serialization.XmlElementAttribute("MaskBand", typeof(MaskBandType), Order = 0)]
        [System.Xml.Serialization.XmlElementAttribute("Metadata", typeof(MetadataType), Order = 0)]
        [System.Xml.Serialization.XmlElementAttribute("PansharpeningOptions", typeof(PansharpeningOptionsType), Order = 0)]
        [System.Xml.Serialization.XmlElementAttribute("SRS", typeof(string), Order = 0)]
        [System.Xml.Serialization.XmlElementAttribute("VRTRasterBand", typeof(VRTRasterBandType), Order = 0)]
        [System.Xml.Serialization.XmlChoiceIdentifierAttribute("ItemsElementName")]
        public object[] Items;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("ItemsElementName", Order = 1)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public ItemsChoiceType5[] ItemsElementName;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string subClass;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "nonNegativeInteger")]
        public string rasterXSize;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "nonNegativeInteger")]
        public string rasterYSize;
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class GCPListType
    {

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("GCP", Order = 0)]
        public GCPType[] GCP;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Projection;
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class GCPType
    {

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Id;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Info;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public double Pixel;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public double Line;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public double X;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public double Y;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public double Z;

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ZSpecified;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public double GCPZ;

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool GCPZSpecified;
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class SpectralBandType
    {

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order = 0)]
        public SourceFilenameType SourceFilename;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order = 1)]
        public string SourceBand;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "nonNegativeInteger")]
        public string dstBand;
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class SourceFilenameType
    {

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public ZeroOrOne relativeToVRT;

        /// <remarks/>
        //[System.Xml.Serialization.XmlIgnoreAttribute()]
        //public bool relativeToVRTSpecified;

        /// <remarks/>
        //[System.Xml.Serialization.XmlAttributeAttribute()]
        //public ZeroOrOne relativetoVRT;

        /// <remarks/>
        //[System.Xml.Serialization.XmlIgnoreAttribute()]
        //public bool relativetoVRTSpecified;

        /// <remarks/>
        //[System.Xml.Serialization.XmlAttributeAttribute()]
        //public OGRBooleanType shared;

        /// <remarks/>
        //[System.Xml.Serialization.XmlIgnoreAttribute()]
        //public bool sharedSpecified;

        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value;
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    public enum ZeroOrOne
    {

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("0")]
        Item0,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        Item1,
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    public enum OGRBooleanType
    {

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        Item1,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("0")]
        Item0,

        /// <remarks/>
        ON,

        /// <remarks/>
        OFF,

        /// <remarks/>
        on,

        /// <remarks/>
        off,

        /// <remarks/>
        YES,

        /// <remarks/>
        NO,

        /// <remarks/>
        yes,

        /// <remarks/>
        no,

        /// <remarks/>
        TRUE,

        /// <remarks/>
        FALSE,

        /// <remarks/>
        @true,

        /// <remarks/>
        @false,

        /// <remarks/>
        True,

        /// <remarks/>
        False,
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class PanchroBandType
    {

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order = 0)]
        public SourceFilenameType SourceFilename;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order = 1)]
        public string SourceBand;
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class AlgorithmOptionsType
    {

        /// <remarks/>
        [System.Xml.Serialization.XmlAnyElementAttribute(Order = 0)]
        public System.Xml.XmlElement[] Any;
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class PansharpeningOptionsType
    {

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order = 0)]
        public string Algorithm;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order = 1)]
        public AlgorithmOptionsType AlgorithmOptions;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order = 2)]
        public string Resampling;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order = 3)]
        public string NumThreads;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order = 4)]
        public string BitDepth;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order = 5)]
        public string NoData;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order = 6)]
        public string SpatialExtentAdjustment;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order = 7)]
        public PanchroBandType PanchroBand;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("SpectralBand", Order = 8)]
        public SpectralBandType[] SpectralBand;
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class GDALWarpOptionsType
    {

        /// <remarks/>
        [System.Xml.Serialization.XmlAnyElementAttribute(Order = 0)]
        public System.Xml.XmlElement[] Any;
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class KernelType
    {

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "nonNegativeInteger")]
        public string Size;

        /// <remarks/>
        public string Coefs;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public ZeroOrOne normalized;

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool normalizedSpecified;
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class KernelFilteredSourceType
    {

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("ColorTableComponent", typeof(string), DataType = "nonNegativeInteger", Order = 0)]
        [System.Xml.Serialization.XmlElementAttribute("DstMax", typeof(double), Order = 0)]
        [System.Xml.Serialization.XmlElementAttribute("DstMin", typeof(double), Order = 0)]
        [System.Xml.Serialization.XmlElementAttribute("DstRect", typeof(RectType), Order = 0)]
        [System.Xml.Serialization.XmlElementAttribute("Exponent", typeof(double), Order = 0)]
        [System.Xml.Serialization.XmlElementAttribute("Kernel", typeof(KernelType), Order = 0)]
        [System.Xml.Serialization.XmlElementAttribute("LUT", typeof(string), Order = 0)]
        [System.Xml.Serialization.XmlElementAttribute("NODATA", typeof(string), Order = 0)]
        [System.Xml.Serialization.XmlElementAttribute("OpenOptions", typeof(OpenOptionsType), Order = 0)]
        [System.Xml.Serialization.XmlElementAttribute("ScaleOffset", typeof(double), Order = 0)]
        [System.Xml.Serialization.XmlElementAttribute("ScaleRatio", typeof(double), Order = 0)]
        [System.Xml.Serialization.XmlElementAttribute("SourceBand", typeof(string), Order = 0)]
        [System.Xml.Serialization.XmlElementAttribute("SourceFilename", typeof(SourceFilenameType), Order = 0)]
        [System.Xml.Serialization.XmlElementAttribute("SourceProperties", typeof(SourcePropertiesType), Order = 0)]
        [System.Xml.Serialization.XmlElementAttribute("SrcMax", typeof(double), Order = 0)]
        [System.Xml.Serialization.XmlElementAttribute("SrcMin", typeof(double), Order = 0)]
        [System.Xml.Serialization.XmlElementAttribute("SrcRect", typeof(RectType), Order = 0)]
        [System.Xml.Serialization.XmlChoiceIdentifierAttribute("ItemsElementName")]
        public object[] Items;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("ItemsElementName", Order = 1)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public ItemsChoiceType3[] ItemsElementName;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string resampling;
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class RectType
    {

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public double xOff;

        /// <remarks/>
        //[System.Xml.Serialization.XmlIgnoreAttribute()]
        //public bool xOffSpecified;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public double yOff;

        /// <remarks/>
        //[System.Xml.Serialization.XmlIgnoreAttribute()]
        //public bool yOffSpecified;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public double xSize;

        /// <remarks/>
        //[System.Xml.Serialization.XmlIgnoreAttribute()]
        //public bool xSizeSpecified;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public double ySize;

        /// <remarks/>
        //[System.Xml.Serialization.XmlIgnoreAttribute()]
        //public bool ySizeSpecified;
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class OpenOptionsType
    {

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("OOI", Order = 0)]
        public OOIType[] OOI;
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class OOIType
    {

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string key;

        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value;
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class SourcePropertiesType
    {

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "nonNegativeInteger")]
        public string RasterXSize;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "nonNegativeInteger")]
        public string RasterYSize;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public DataTypeType DataType;

        /// <remarks/>
        //[System.Xml.Serialization.XmlIgnoreAttribute()]
        //public bool DataTypeSpecified;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "nonNegativeInteger")]
        public string BlockXSize;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "nonNegativeInteger")]
        public string BlockYSize;
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    public enum DataTypeType
    {

        /// <remarks/>
        Byte,

        /// <remarks/>
        UInt16,

        /// <remarks/>
        Int16,

        /// <remarks/>
        UInt32,

        /// <remarks/>
        Int32,

        /// <remarks/>
        Float32,

        /// <remarks/>
        Float64,

        /// <remarks/>
        CInt16,

        /// <remarks/>
        CInt32,

        /// <remarks/>
        CFloat32,

        /// <remarks/>
        CFloat64,
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(IncludeInSchema = false)]
    public enum ItemsChoiceType3
    {

        /// <remarks/>
        ColorTableComponent,

        /// <remarks/>
        DstMax,

        /// <remarks/>
        DstMin,

        /// <remarks/>
        DstRect,

        /// <remarks/>
        Exponent,

        /// <remarks/>
        Kernel,

        /// <remarks/>
        LUT,

        /// <remarks/>
        NODATA,

        /// <remarks/>
        OpenOptions,

        /// <remarks/>
        ScaleOffset,

        /// <remarks/>
        ScaleRatio,

        /// <remarks/>
        SourceBand,

        /// <remarks/>
        SourceFilename,

        /// <remarks/>
        SourceProperties,

        /// <remarks/>
        SrcMax,

        /// <remarks/>
        SrcMin,

        /// <remarks/>
        SrcRect,
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class ComplexSourceType
    {

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("ColorTableComponent", typeof(string), DataType = "nonNegativeInteger", Order = 0)]
        [System.Xml.Serialization.XmlElementAttribute("DstMax", typeof(double), Order = 0)]
        [System.Xml.Serialization.XmlElementAttribute("DstMin", typeof(double), Order = 0)]
        [System.Xml.Serialization.XmlElementAttribute("DstRect", typeof(RectType), Order = 0)]
        [System.Xml.Serialization.XmlElementAttribute("Exponent", typeof(double), Order = 0)]
        [System.Xml.Serialization.XmlElementAttribute("LUT", typeof(string), Order = 0)]
        [System.Xml.Serialization.XmlElementAttribute("NODATA", typeof(string), Order = 0)]
        [System.Xml.Serialization.XmlElementAttribute("OpenOptions", typeof(OpenOptionsType), Order = 0)]
        [System.Xml.Serialization.XmlElementAttribute("ScaleOffset", typeof(double), Order = 0)]
        [System.Xml.Serialization.XmlElementAttribute("ScaleRatio", typeof(double), Order = 0)]
        [System.Xml.Serialization.XmlElementAttribute("SourceBand", typeof(string), Order = 0)]
        [System.Xml.Serialization.XmlElementAttribute("SourceFilename", typeof(SourceFilenameType), Order = 0)]
        [System.Xml.Serialization.XmlElementAttribute("SourceProperties", typeof(SourcePropertiesType), Order = 0)]
        [System.Xml.Serialization.XmlElementAttribute("SrcMax", typeof(double), Order = 0)]
        [System.Xml.Serialization.XmlElementAttribute("SrcMin", typeof(double), Order = 0)]
        [System.Xml.Serialization.XmlElementAttribute("SrcRect", typeof(RectType), Order = 0)]
        [System.Xml.Serialization.XmlChoiceIdentifierAttribute("ItemsElementName")]
        public object[] Items;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("ItemsElementName", Order = 1)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public ItemsChoiceType2[] ItemsElementName;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string resampling;
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(IncludeInSchema = false)]
    public enum ItemsChoiceType2
    {

        /// <remarks/>
        ColorTableComponent,

        /// <remarks/>
        DstMax,

        /// <remarks/>
        DstMin,

        /// <remarks/>
        DstRect,

        /// <remarks/>
        Exponent,

        /// <remarks/>
        LUT,

        /// <remarks/>
        NODATA,

        /// <remarks/>
        OpenOptions,

        /// <remarks/>
        ScaleOffset,

        /// <remarks/>
        ScaleRatio,

        /// <remarks/>
        SourceBand,

        /// <remarks/>
        SourceFilename,

        /// <remarks/>
        SourceProperties,

        /// <remarks/>
        SrcMax,

        /// <remarks/>
        SrcMin,

        /// <remarks/>
        SrcRect,
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class SimpleSourceType
    {

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("DstRect", typeof(RectType), Order = 0)]
        [System.Xml.Serialization.XmlElementAttribute("OpenOptions", typeof(OpenOptionsType), Order = 0)]
        [System.Xml.Serialization.XmlElementAttribute("SourceBand", typeof(string), Order = 0)]
        [System.Xml.Serialization.XmlElementAttribute("SourceFilename", typeof(SourceFilenameType), Order = 0)]
        [System.Xml.Serialization.XmlElementAttribute("SourceProperties", typeof(SourcePropertiesType), Order = 0)]
        [System.Xml.Serialization.XmlElementAttribute("SrcRect", typeof(RectType), Order = 0)]
        [System.Xml.Serialization.XmlChoiceIdentifierAttribute("ItemsElementName")]
        public object[] Items;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("ItemsElementName", Order = 1)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public ItemsChoiceType1[] ItemsElementName;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string resampling;
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(IncludeInSchema = false)]
    public enum ItemsChoiceType1
    {

        /// <remarks/>
        DstRect,

        /// <remarks/>
        OpenOptions,

        /// <remarks/>
        SourceBand,

        /// <remarks/>
        SourceFilename,

        /// <remarks/>
        SourceProperties,

        /// <remarks/>
        SrcRect,
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class HistItemType
    {

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Approximate", typeof(ZeroOrOne), Order = 0)]
        [System.Xml.Serialization.XmlElementAttribute("BucketCount", typeof(string), DataType = "integer", Order = 0)]
        [System.Xml.Serialization.XmlElementAttribute("HistCounts", typeof(string), Order = 0)]
        [System.Xml.Serialization.XmlElementAttribute("HistMax", typeof(double), Order = 0)]
        [System.Xml.Serialization.XmlElementAttribute("HistMin", typeof(double), Order = 0)]
        [System.Xml.Serialization.XmlElementAttribute("IncludeOutOfRange", typeof(ZeroOrOne), Order = 0)]
        [System.Xml.Serialization.XmlChoiceIdentifierAttribute("ItemsElementName")]
        public object[] Items;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("ItemsElementName", Order = 1)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public ItemsChoiceType[] ItemsElementName;
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(IncludeInSchema = false)]
    public enum ItemsChoiceType
    {

        /// <remarks/>
        Approximate,

        /// <remarks/>
        BucketCount,

        /// <remarks/>
        HistCounts,

        /// <remarks/>
        HistMax,

        /// <remarks/>
        HistMin,

        /// <remarks/>
        IncludeOutOfRange,
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class HistogramsType
    {

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("HistItem", Order = 0)]
        public HistItemType[] HistItem;
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class MaskBandType
    {

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order = 0)]
        public VRTRasterBandType VRTRasterBand;
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class VRTRasterBandType
    {

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("AveragedSource", typeof(SimpleSourceType), Order = 0)]
        [System.Xml.Serialization.XmlElementAttribute("BufferRadius", typeof(string), DataType = "nonNegativeInteger", Order = 0)]
        [System.Xml.Serialization.XmlElementAttribute("ByteOrder", typeof(string), Order = 0)]
        [System.Xml.Serialization.XmlElementAttribute("CategoryNames", typeof(CategoryNamesType), Order = 0)]
        [System.Xml.Serialization.XmlElementAttribute("ColorInterp", typeof(ColorInterpType), Order = 0)]
        [System.Xml.Serialization.XmlElementAttribute("ColorTable", typeof(ColorTableType), Order = 0)]
        [System.Xml.Serialization.XmlElementAttribute("ComplexSource", typeof(ComplexSourceType), Order = 0)]
        [System.Xml.Serialization.XmlElementAttribute("Description", typeof(string), Order = 0)]
        [System.Xml.Serialization.XmlElementAttribute("HideNoDataValue", typeof(ZeroOrOne), Order = 0)]
        [System.Xml.Serialization.XmlElementAttribute("Histograms", typeof(HistogramsType), Order = 0)]
        [System.Xml.Serialization.XmlElementAttribute("ImageOffset", typeof(string), DataType = "integer", Order = 0)]
        [System.Xml.Serialization.XmlElementAttribute("KernelFilteredSource", typeof(KernelFilteredSourceType), Order = 0)]
        [System.Xml.Serialization.XmlElementAttribute("LineOffset", typeof(string), DataType = "integer", Order = 0)]
        [System.Xml.Serialization.XmlElementAttribute("MaskBand", typeof(MaskBandType), Order = 0)]
        [System.Xml.Serialization.XmlElementAttribute("Metadata", typeof(MetadataType), Order = 0)]
        [System.Xml.Serialization.XmlElementAttribute("NoDataValue", typeof(string), Order = 0)]
        [System.Xml.Serialization.XmlElementAttribute("NodataValue", typeof(double), Order = 0)]
        [System.Xml.Serialization.XmlElementAttribute("Offset", typeof(double), Order = 0)]
        [System.Xml.Serialization.XmlElementAttribute("Overview", typeof(OverviewType), Order = 0)]
        [System.Xml.Serialization.XmlElementAttribute("PixelFunctionArguments", typeof(VRTRasterBandTypePixelFunctionArguments), Order = 0)]
        [System.Xml.Serialization.XmlElementAttribute("PixelFunctionCode", typeof(string), Order = 0)]
        [System.Xml.Serialization.XmlElementAttribute("PixelFunctionLanguage", typeof(string), Order = 0)]
        [System.Xml.Serialization.XmlElementAttribute("PixelFunctionType", typeof(string), Order = 0)]
        [System.Xml.Serialization.XmlElementAttribute("PixelOffset", typeof(string), DataType = "integer", Order = 0)]
        [System.Xml.Serialization.XmlElementAttribute("Scale", typeof(double), Order = 0)]
        [System.Xml.Serialization.XmlElementAttribute("SimpleSource", typeof(SimpleSourceType), Order = 0)]
        [System.Xml.Serialization.XmlElementAttribute("SourceFilename", typeof(SourceFilenameType), Order = 0)]
        [System.Xml.Serialization.XmlElementAttribute("SourceTransferType", typeof(DataTypeType), Order = 0)]
        [System.Xml.Serialization.XmlElementAttribute("UnitType", typeof(string), Order = 0)]
        [System.Xml.Serialization.XmlChoiceIdentifierAttribute("ItemsElementName")]
        public object[] Items;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("ItemsElementName", Order = 1)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public ItemsChoiceType4[] ItemsElementName;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public DataTypeType dataType;

        /// <remarks/>
        //[System.Xml.Serialization.XmlIgnoreAttribute()]
        //public bool dataTypeSpecified;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public uint band;

        /// <remarks/>
        //[System.Xml.Serialization.XmlIgnoreAttribute()]
        //public bool bandSpecified;

        /// <remarks/>
        //[System.Xml.Serialization.XmlAttributeAttribute()]
        //public VRTRasterBandSubClassType subClass;

        /// <remarks/>
        //[System.Xml.Serialization.XmlIgnoreAttribute()]
        //public bool subClassSpecified;

        /// <remarks/>
        //[System.Xml.Serialization.XmlAttributeAttribute()]
        //public uint BlockXSize;

        /// <remarks/>
        //[System.Xml.Serialization.XmlIgnoreAttribute()]
        //public bool BlockXSizeSpecified;

        /// <remarks/>
        //[System.Xml.Serialization.XmlAttributeAttribute()]
        //public uint BlockYSize;

        /// <remarks/>
        //[System.Xml.Serialization.XmlIgnoreAttribute()]
        //public bool BlockYSizeSpecified;
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class CategoryNamesType
    {

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Category", Order = 0)]
        public string[] Category;
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    public enum ColorInterpType
    {

        /// <remarks/>
        Gray,

        /// <remarks/>
        Palette,

        /// <remarks/>
        Red,

        /// <remarks/>
        Green,

        /// <remarks/>
        Blue,

        /// <remarks/>
        Alpha,

        /// <remarks/>
        Hue,

        /// <remarks/>
        Saturation,

        /// <remarks/>
        Lightness,

        /// <remarks/>
        Cyan,

        /// <remarks/>
        Magenta,

        /// <remarks/>
        Yellow,

        /// <remarks/>
        Black,

        /// <remarks/>
        YCbCr_Y,

        /// <remarks/>
        YCbCr_Cb,

        /// <remarks/>
        YCbCr_Cr,

        /// <remarks/>
        Undefined,
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class ColorTableType
    {

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Entry", Order = 0)]
        public ColorTableEntryType[] Entry;
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class ColorTableEntryType
    {

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public uint c1;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public uint c2;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public uint c3;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public uint c4;

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool c4Specified;
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class MetadataType
    {

        /// <remarks/>
        [System.Xml.Serialization.XmlAnyElementAttribute(Order = 0)]
        public System.Xml.XmlElement[] Any;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string domain;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string format;
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class OverviewType
    {

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("SourceBand", typeof(string), Order = 0)]
        [System.Xml.Serialization.XmlElementAttribute("SourceFilename", typeof(SourceFilenameType), Order = 0)]
        public object[] Items;
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class VRTRasterBandTypePixelFunctionArguments
    {

        /// <remarks/>
        [System.Xml.Serialization.XmlAnyAttributeAttribute()]
        public System.Xml.XmlAttribute[] AnyAttr;
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(IncludeInSchema = false)]
    public enum ItemsChoiceType4
    {

        /// <remarks/>
        AveragedSource,

        /// <remarks/>
        BufferRadius,

        /// <remarks/>
        ByteOrder,

        /// <remarks/>
        CategoryNames,

        /// <remarks/>
        ColorInterp,

        /// <remarks/>
        ColorTable,

        /// <remarks/>
        ComplexSource,

        /// <remarks/>
        Description,

        /// <remarks/>
        HideNoDataValue,

        /// <remarks/>
        Histograms,

        /// <remarks/>
        ImageOffset,

        /// <remarks/>
        KernelFilteredSource,

        /// <remarks/>
        LineOffset,

        /// <remarks/>
        MaskBand,

        /// <remarks/>
        Metadata,

        /// <remarks/>
        NoDataValue,

        /// <remarks/>
        NodataValue,

        /// <remarks/>
        Offset,

        /// <remarks/>
        Overview,

        /// <remarks/>
        PixelFunctionArguments,

        /// <remarks/>
        PixelFunctionCode,

        /// <remarks/>
        PixelFunctionLanguage,

        /// <remarks/>
        PixelFunctionType,

        /// <remarks/>
        PixelOffset,

        /// <remarks/>
        Scale,

        /// <remarks/>
        SimpleSource,

        /// <remarks/>
        SourceFilename,

        /// <remarks/>
        SourceTransferType,

        /// <remarks/>
        UnitType,
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    public enum VRTRasterBandSubClassType
    {

        /// <remarks/>
        VRTWarpedRasterBand,

        /// <remarks/>
        VRTDerivedRasterBand,

        /// <remarks/>
        VRTRawRasterBand,

        /// <remarks/>
        VRTPansharpenedRasterBand,
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(IncludeInSchema = false)]
    public enum ItemsChoiceType5
    {

        /// <remarks/>
        BlockXSize,

        /// <remarks/>
        BlockYSize,

        /// <remarks/>
        GCPList,

        /// <remarks/>
        GDALWarpOptions,

        /// <remarks/>
        GeoTransform,

        /// <remarks/>
        MaskBand,

        /// <remarks/>
        Metadata,

        /// <remarks/>
        PansharpeningOptions,

        /// <remarks/>
        SRS,

        /// <remarks/>
        VRTRasterBand,
    }

}
