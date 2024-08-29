using Sign.itext.error_messages;
using System.Reflection;
using System.Text;

namespace Sign.itext.pdf
{
    public class PdfName : PdfObject, IComparable<PdfName>
    {
        public static readonly PdfName _3D;

        public static readonly PdfName A;

        public static readonly PdfName A85;

        public static readonly PdfName AA;

        public static readonly PdfName ABSOLUTECOLORIMETRIC;

        public static readonly PdfName AC;

        public static readonly PdfName ACROFORM;

        public static readonly PdfName ACTION;

        public static readonly PdfName ACTIVATION;

        public static readonly PdfName ADBE;

        public static readonly PdfName ACTUALTEXT;

        public static readonly PdfName ADBE_PKCS7_DETACHED;

        public static readonly PdfName ADBE_PKCS7_S4;

        public static readonly PdfName ADBE_PKCS7_S5;

        public static readonly PdfName ADBE_PKCS7_SHA1;

        public static readonly PdfName ADBE_X509_RSA_SHA1;

        public static readonly PdfName ADOBE_PPKLITE;

        public static readonly PdfName ADOBE_PPKMS;

        public static readonly PdfName AESV2;

        public static readonly PdfName AESV3;

        public static readonly PdfName AFRELATIONSHIP;

        public static readonly PdfName AHX;

        public static readonly PdfName AIS;

        public static readonly PdfName ALL;

        public static readonly PdfName ALLPAGES;

        public static readonly PdfName ALT;

        public static readonly PdfName ALTERNATE;

        public static readonly PdfName ALTERNATEPRESENTATION;

        public static readonly PdfName ALTERNATES;

        public static readonly PdfName AND;

        public static readonly PdfName ANIMATION;

        public static readonly PdfName ANNOT;

        public static readonly PdfName ANNOTS;

        public static readonly PdfName ANTIALIAS;

        public static readonly PdfName AP;

        public static readonly PdfName APP;

        public static readonly PdfName APPDEFAULT;

        public static readonly PdfName ART;

        public static readonly PdfName ARTBOX;

        public static readonly PdfName ARTIFACT;

        public static readonly PdfName ASCENT;

        public static readonly PdfName AS;

        public static readonly PdfName ASCII85DECODE;

        public static readonly PdfName ASCIIHEXDECODE;

        public static readonly PdfName ASSET;

        public static readonly PdfName ASSETS;

        public static PdfName ATTACHED;

        public static readonly PdfName AUTHEVENT;

        public static readonly PdfName AUTHOR;

        public static readonly PdfName B;

        public static readonly PdfName BACKGROUND;

        public static readonly PdfName BACKGROUNDCOLOR;

        public static readonly PdfName BASEENCODING;

        public static readonly PdfName BASEFONT;

        public static readonly PdfName BASEVERSION;

        public static readonly PdfName BBOX;

        public static readonly PdfName BC;

        public static readonly PdfName BG;

        public static readonly PdfName BIBENTRY;

        public static readonly PdfName BIGFIVE;

        public static readonly PdfName BINDING;

        public static readonly PdfName BINDINGMATERIALNAME;

        public static readonly PdfName BITSPERCOMPONENT;

        public static readonly PdfName BITSPERSAMPLE;

        public static readonly PdfName BL;

        public static readonly PdfName BLACKIS1;

        public static readonly PdfName BLACKPOINT;

        public static readonly PdfName BLOCKQUOTE;

        public static readonly PdfName BLEEDBOX;

        public static readonly PdfName BLINDS;

        public static readonly PdfName BM;

        public static readonly PdfName BORDER;

        public static readonly PdfName BOTH;

        public static readonly PdfName BOUNDS;

        public static readonly PdfName BOX;

        public static readonly PdfName BS;

        public static readonly PdfName BTN;

        public static readonly PdfName BYTERANGE;

        public static readonly PdfName C;

        public static readonly PdfName C0;

        public static readonly PdfName C1;

        public static readonly PdfName CA;

        public static readonly PdfName ca;

        public static readonly PdfName CALGRAY;

        public static readonly PdfName CALRGB;

        public static readonly PdfName CAPHEIGHT;

        public static readonly PdfName CARET;

        public static readonly PdfName CAPTION;

        public static readonly PdfName CATALOG;

        public static readonly PdfName CATEGORY;

        public static readonly PdfName CB;

        public static readonly PdfName CCITTFAXDECODE;

        public static readonly PdfName CENTER;

        public static readonly PdfName CENTERWINDOW;

        public static readonly PdfName CERT;

        public static readonly PdfName CERTS;

        public static readonly PdfName CF;

        public static readonly PdfName CFM;

        public static readonly PdfName CH;

        public static readonly PdfName CHARPROCS;

        public static readonly PdfName CHECKSUM;

        public static readonly PdfName CI;

        public static readonly PdfName CIDFONTTYPE0;

        public static readonly PdfName CIDFONTTYPE2;

        public static readonly PdfName CIDSET;

        public static readonly PdfName CIDSYSTEMINFO;

        public static readonly PdfName CIDTOGIDMAP;

        public static readonly PdfName CIRCLE;

        public static readonly PdfName CLASSMAP;

        public static readonly PdfName CLOUD;

        public static readonly PdfName CMD;

        public static readonly PdfName CO;

        public static readonly PdfName CODE;

        public static readonly PdfName COLOR;

        public static readonly PdfName COLORANTS;

        public static readonly PdfName COLORS;

        public static readonly PdfName COLORSPACE;

        public static readonly PdfName COLORTRANSFORM;

        public static readonly PdfName COLLECTION;

        public static readonly PdfName COLLECTIONFIELD;

        public static readonly PdfName COLLECTIONITEM;

        public static readonly PdfName COLLECTIONSCHEMA;

        public static readonly PdfName COLLECTIONSORT;

        public static readonly PdfName COLLECTIONSUBITEM;

        public static readonly PdfName COLSPAN;

        public static readonly PdfName COLUMN;

        public static readonly PdfName COLUMNS;

        public static readonly PdfName CONDITION;

        public static readonly PdfName CONFIGS;

        public static readonly PdfName CONFIGURATION;

        public static readonly PdfName CONFIGURATIONS;

        public static readonly PdfName CONTACTINFO;

        public static readonly PdfName CONTENT;

        public static readonly PdfName CONTENTS;

        public static readonly PdfName COORDS;

        public static readonly PdfName COUNT;

        public static readonly PdfName COURIER;

        public static readonly PdfName COURIER_BOLD;

        public static readonly PdfName COURIER_OBLIQUE;

        public static readonly PdfName COURIER_BOLDOBLIQUE;

        public static readonly PdfName CREATIONDATE;

        public static readonly PdfName CREATOR;

        public static readonly PdfName CREATORINFO;

        public static readonly PdfName CRL;

        public static readonly PdfName CRLS;

        public static readonly PdfName CROPBOX;

        public static readonly PdfName CRYPT;

        public static readonly PdfName CS;

        public static readonly PdfName CUEPOINT;

        public static readonly PdfName CUEPOINTS;

        public static readonly PdfName CYX;

        public static readonly PdfName D;

        public static readonly PdfName DA;

        public static readonly PdfName DATA;

        public static readonly PdfName DC;

        public static readonly PdfName DCS;

        public static readonly PdfName DCTDECODE;

        public static readonly PdfName DECIMAL;

        public static readonly PdfName DEACTIVATION;

        public static readonly PdfName DECODE;

        public static readonly PdfName DECODEPARMS;

        public static readonly PdfName DEFAULT;

        public static readonly PdfName DEFAULTCRYPTFILTER;

        public static readonly PdfName DEFAULTCMYK;

        public static readonly PdfName DEFAULTGRAY;

        public static readonly PdfName DEFAULTRGB;

        public static readonly PdfName DESC;

        public static readonly PdfName DESCENDANTFONTS;

        public static readonly PdfName DESCENT;

        public static readonly PdfName DEST;

        public static readonly PdfName DESTOUTPUTPROFILE;

        public static readonly PdfName DESTS;

        public static readonly PdfName DEVICEGRAY;

        public static readonly PdfName DEVICERGB;

        public static readonly PdfName DEVICECMYK;

        public static readonly PdfName DEVICEN;

        public static readonly PdfName DI;

        public static readonly PdfName DIFFERENCES;

        public static readonly PdfName DISSOLVE;

        public static readonly PdfName DIRECTION;

        public static readonly PdfName DISPLAYDOCTITLE;

        public static readonly PdfName DIV;

        public static readonly PdfName DL;

        public static readonly PdfName DM;

        public static readonly PdfName DOS;

        public static readonly PdfName DOCMDP;

        public static readonly PdfName DOCOPEN;

        public static readonly PdfName DOCTIMESTAMP;

        public static readonly PdfName DOCUMENT;

        public static readonly PdfName DOMAIN;

        public static readonly PdfName DP;

        public static readonly PdfName DR;

        public static readonly PdfName DS;

        public static readonly PdfName DSS;

        public static readonly PdfName DUR;

        public static readonly PdfName DUPLEX;

        public static readonly PdfName DUPLEXFLIPSHORTEDGE;

        public static readonly PdfName DUPLEXFLIPLONGEDGE;

        public static readonly PdfName DV;

        public static readonly PdfName DW;

        public static readonly PdfName E;

        public static readonly PdfName EARLYCHANGE;

        public static readonly PdfName EF;

        public static readonly PdfName EFF;

        public static readonly PdfName EFOPEN;

        public static readonly PdfName EMBEDDED;

        public static readonly PdfName EMBEDDEDFILE;

        public static readonly PdfName EMBEDDEDFILES;

        public static readonly PdfName ENCODE;

        public static readonly PdfName ENCODEDBYTEALIGN;

        public static readonly PdfName ENCODING;

        public static readonly PdfName ENCRYPT;

        public static readonly PdfName ENCRYPTMETADATA;

        public static readonly PdfName END;

        public static readonly PdfName ENDINDENT;

        public static readonly PdfName ENDOFBLOCK;

        public static readonly PdfName ENDOFLINE;

        public static readonly PdfName EPSG;

        public static readonly PdfName ESIC;

        public static readonly PdfName ETSI_CADES_DETACHED;

        public static readonly PdfName ETSI_RFC3161;

        public static readonly PdfName EXCLUDE;

        public static readonly PdfName EXTEND;

        public static readonly PdfName EXTENSIONS;

        public static readonly PdfName EXTENSIONLEVEL;

        public static readonly PdfName EXTGSTATE;

        public static readonly PdfName EXPORT;

        public static readonly PdfName EXPORTSTATE;

        public static readonly PdfName EVENT;

        public static readonly PdfName F;

        public static readonly PdfName FAR;

        public static readonly PdfName FB;

        public static readonly PdfName FD;

        public static readonly PdfName FDECODEPARMS;

        public static readonly PdfName FDF;

        public static readonly PdfName FF;

        public static readonly PdfName FFILTER;

        public static readonly PdfName FG;

        public static readonly PdfName FIELDMDP;

        public static readonly PdfName FIELDS;

        public static readonly PdfName FIGURE;

        public static readonly PdfName FILEATTACHMENT;

        public static readonly PdfName FILESPEC;

        public static readonly PdfName FILTER;

        public static readonly PdfName FIRST;

        public static readonly PdfName FIRSTCHAR;

        public static readonly PdfName FIRSTPAGE;

        public static readonly PdfName FIT;

        public static readonly PdfName FITH;

        public static readonly PdfName FITV;

        public static readonly PdfName FITR;

        public static readonly PdfName FITB;

        public static readonly PdfName FITBH;

        public static readonly PdfName FITBV;

        public static readonly PdfName FITWINDOW;

        public static readonly PdfName FL;

        public static readonly PdfName FLAGS;

        public static readonly PdfName FLASH;

        public static readonly PdfName FLASHVARS;

        public static readonly PdfName FLATEDECODE;

        public static readonly PdfName FO;

        public static readonly PdfName FONT;

        public static readonly PdfName FONTBBOX;

        public static readonly PdfName FONTDESCRIPTOR;

        public static readonly PdfName FONTFAMILY;

        public static readonly PdfName FONTFILE;

        public static readonly PdfName FONTFILE2;

        public static readonly PdfName FONTFILE3;

        public static readonly PdfName FONTMATRIX;

        public static readonly PdfName FONTNAME;

        public static readonly PdfName FONTWEIGHT;

        public static readonly PdfName FOREGROUND;

        public static readonly PdfName FORM;

        public static readonly PdfName FORMTYPE;

        public static readonly PdfName FORMULA;

        public static readonly PdfName FREETEXT;

        public static readonly PdfName FRM;

        public static readonly PdfName FS;

        public static readonly PdfName FT;

        public static readonly PdfName FULLSCREEN;

        public static readonly PdfName FUNCTION;

        public static readonly PdfName FUNCTIONS;

        public static readonly PdfName FUNCTIONTYPE;

        public static readonly PdfName GAMMA;

        public static readonly PdfName GBK;

        public static readonly PdfName GCS;

        public static readonly PdfName GEO;

        public static readonly PdfName GEOGCS;

        public static readonly PdfName GLITTER;

        public static readonly PdfName GOTO;

        public static readonly PdfName GOTO3DVIEW;

        public static readonly PdfName GOTOE;

        public static readonly PdfName GOTOR;

        public static readonly PdfName GPTS;

        public static readonly PdfName GROUP;

        public static readonly PdfName GTS_PDFA1;

        public static readonly PdfName GTS_PDFX;

        public static readonly PdfName GTS_PDFXVERSION;

        public static readonly PdfName H;

        public static readonly PdfName H1;

        public static readonly PdfName H2;

        public static readonly PdfName H3;

        public static readonly PdfName H4;

        public static readonly PdfName H5;

        public static readonly PdfName H6;

        public static readonly PdfName HALFTONENAME;

        public static readonly PdfName HALFTONETYPE;

        public static readonly PdfName HALIGN;

        public static readonly PdfName HEADERS;

        public static readonly PdfName HEIGHT;

        public static readonly PdfName HELV;

        public static readonly PdfName HELVETICA;

        public static readonly PdfName HELVETICA_BOLD;

        public static readonly PdfName HELVETICA_OBLIQUE;

        public static readonly PdfName HELVETICA_BOLDOBLIQUE;

        public static readonly PdfName HF;

        public static readonly PdfName HID;

        public static readonly PdfName HIDE;

        public static readonly PdfName HIDEMENUBAR;

        public static readonly PdfName HIDETOOLBAR;

        public static readonly PdfName HIDEWINDOWUI;

        public static readonly PdfName HIGHLIGHT;

        public static readonly PdfName HOFFSET;

        public static readonly PdfName HT;

        public static readonly PdfName HTP;

        public static readonly PdfName I;

        public static readonly PdfName IC;

        public static readonly PdfName ICCBASED;

        public static readonly PdfName ID;

        public static readonly PdfName IDENTITY;

        public static readonly PdfName IF;

        public static readonly PdfName IM;

        public static readonly PdfName IMAGE;

        public static readonly PdfName IMAGEB;

        public static readonly PdfName IMAGEC;

        public static readonly PdfName IMAGEI;

        public static readonly PdfName IMAGEMASK;

        public static readonly PdfName INCLUDE;

        public static readonly PdfName IND;

        public static readonly PdfName INDEX;

        public static readonly PdfName INDEXED;

        public static readonly PdfName INFO;

        public static readonly PdfName INK;

        public static readonly PdfName INKLIST;

        public static readonly PdfName INSTANCES;

        public static readonly PdfName IMPORTDATA;

        public static readonly PdfName INTENT;

        public static readonly PdfName INTERPOLATE;

        public static readonly PdfName ISMAP;

        public static readonly PdfName IRT;

        public static readonly PdfName ITALICANGLE;

        public static readonly PdfName ITXT;

        public static readonly PdfName IX;

        public static readonly PdfName JAVASCRIPT;

        public static readonly PdfName JBIG2DECODE;

        public static readonly PdfName JBIG2GLOBALS;

        public static readonly PdfName JPXDECODE;

        public static readonly PdfName JS;

        public static readonly PdfName JUSTIFY;

        public static readonly PdfName K;

        public static readonly PdfName KEYWORDS;

        public static readonly PdfName KIDS;

        public static readonly PdfName L;

        public static readonly PdfName L2R;

        public static readonly PdfName LAB;

        public static readonly PdfName LANG;

        public static readonly PdfName LANGUAGE;

        public static readonly PdfName LAST;

        public static readonly PdfName LASTCHAR;

        public static readonly PdfName LASTPAGE;

        public static readonly PdfName LAUNCH;

        public static readonly PdfName LAYOUT;

        public static readonly PdfName LBL;

        public static readonly PdfName LBODY;

        public static readonly PdfName LENGTH;

        public static readonly PdfName LENGTH1;

        public static readonly PdfName LI;

        public static readonly PdfName LIMITS;

        public static readonly PdfName LINE;

        public static readonly PdfName LINEAR;

        public static readonly PdfName LINEHEIGHT;

        public static readonly PdfName LINK;

        public static readonly PdfName LIST;

        public static readonly PdfName LISTMODE;

        public static readonly PdfName LISTNUMBERING;

        public static readonly PdfName LOCATION;

        public static readonly PdfName LOCK;

        public static readonly PdfName LOCKED;

        public static readonly PdfName LOWERALPHA;

        public static readonly PdfName LOWERROMAN;

        public static readonly PdfName LPTS;

        public static readonly PdfName LZWDECODE;

        public static readonly PdfName M;

        public static readonly PdfName MAC;

        public static readonly PdfName MATERIAL;

        public static readonly PdfName MATRIX;

        public static readonly PdfName MAC_EXPERT_ENCODING;

        public static readonly PdfName MAC_ROMAN_ENCODING;

        public static readonly PdfName MARKED;

        public static readonly PdfName MARKINFO;

        public static readonly PdfName MASK;

        public static readonly PdfName MAX_LOWER_CASE;

        public static readonly PdfName MAX_CAMEL_CASE;

        public static readonly PdfName MAXLEN;

        public static readonly PdfName MEDIABOX;

        public static readonly PdfName MCID;

        public static readonly PdfName MCR;

        public static readonly PdfName MEASURE;

        public static readonly PdfName METADATA;

        public static readonly PdfName MIN_LOWER_CASE;

        public static readonly PdfName MIN_CAMEL_CASE;

        public static readonly PdfName MK;

        public static readonly PdfName MMTYPE1;

        public static readonly PdfName MODDATE;

        public static readonly PdfName MOVIE;

        public static readonly PdfName N;

        public static readonly PdfName N0;

        public static readonly PdfName N1;

        public static readonly PdfName N2;

        public static readonly PdfName N3;

        public static readonly PdfName N4;

        public new static readonly PdfName NAME;

        public static readonly PdfName NAMED;

        public static readonly PdfName NAMES;

        public static readonly PdfName NAVIGATION;

        public static readonly PdfName NAVIGATIONPANE;

        public static readonly PdfName NCHANNEL;

        public static readonly PdfName NEAR;

        public static readonly PdfName NEEDAPPEARANCES;

        public static readonly PdfName NEEDRENDERING;

        public static readonly PdfName NEWWINDOW;

        public static readonly PdfName NEXT;

        public static readonly PdfName NEXTPAGE;

        public static readonly PdfName NM;

        public static readonly PdfName NONE;

        public static readonly PdfName NONFULLSCREENPAGEMODE;

        public static readonly PdfName NONSTRUCT;

        public static readonly PdfName NOT;

        public static readonly PdfName NOTE;

        public static readonly PdfName NUMBERFORMAT;

        public static readonly PdfName NUMCOPIES;

        public static readonly PdfName NUMS;

        public static readonly PdfName O;

        public static readonly PdfName OBJ;

        public static readonly PdfName OBJR;

        public static readonly PdfName OBJSTM;

        public static readonly PdfName OC;

        public static readonly PdfName OCG;

        public static readonly PdfName OCGS;

        public static readonly PdfName OCMD;

        public static readonly PdfName OCPROPERTIES;

        public static readonly PdfName OCSP;

        public static readonly PdfName OCSPS;

        public static readonly PdfName OE;

        public static readonly PdfName Off_;

        public static readonly PdfName OFF;

        public static readonly PdfName ON;

        public static readonly PdfName ONECOLUMN;

        public static readonly PdfName OPEN;

        public static readonly PdfName OPENACTION;

        public static readonly PdfName OP;

        public static readonly PdfName op_;

        public static readonly PdfName OPI;

        public static readonly PdfName OPM;

        public static readonly PdfName OPT;

        public static readonly PdfName OR;

        public static readonly PdfName ORDER;

        public static readonly PdfName ORDERING;

        public static readonly PdfName ORG;

        public static readonly PdfName OSCILLATING;

        public static readonly PdfName OUTLINES;

        public static readonly PdfName OUTPUTCONDITION;

        public static readonly PdfName OUTPUTCONDITIONIDENTIFIER;

        public static readonly PdfName OUTPUTINTENT;

        public static readonly PdfName OUTPUTINTENTS;

        public static readonly PdfName OVERLAYTEXT;

        public static readonly PdfName P;

        public static readonly PdfName PAGE;

        public static readonly PdfName PAGEELEMENT;

        public static readonly PdfName PAGELABELS;

        public static readonly PdfName PAGELAYOUT;

        public static readonly PdfName PAGEMODE;

        public static readonly PdfName PAGES;

        public static readonly PdfName PAINTTYPE;

        public static readonly PdfName PANOSE;

        public static readonly PdfName PARAMS;

        public static readonly PdfName PARENT;

        public static readonly PdfName PARENTTREE;

        public static readonly PdfName PARENTTREENEXTKEY;

        public static readonly PdfName PART;

        public static readonly PdfName PASSCONTEXTCLICK;

        public static readonly PdfName PATTERN;

        public static readonly PdfName PATTERNTYPE;

        public static readonly PdfName PB;

        public static readonly PdfName PC;

        public static readonly PdfName PDF;

        public static readonly PdfName PDFDOCENCODING;

        public static readonly PdfName PDU;

        public static readonly PdfName PERCEPTUAL;

        public static readonly PdfName PERMS;

        public static readonly PdfName PG;

        public static readonly PdfName PI;

        public static readonly PdfName PICKTRAYBYPDFSIZE;

        public static readonly PdfName PIECEINFO;

        public static readonly PdfName PLAYCOUNT;

        public static readonly PdfName PO;

        public static readonly PdfName POLYGON;

        public static readonly PdfName POLYLINE;

        public static readonly PdfName POPUP;

        public static readonly PdfName POSITION;

        public static readonly PdfName PREDICTOR;

        public static readonly PdfName PREFERRED;

        public static readonly PdfName PRESENTATION;

        public static readonly PdfName PRESERVERB;

        public static readonly PdfName PRESSTEPS;

        public static readonly PdfName PREV;

        public static readonly PdfName PREVPAGE;

        public static readonly PdfName PRINT;

        public static readonly PdfName PRINTAREA;

        public static readonly PdfName PRINTCLIP;

        public static readonly PdfName PRINTERMARK;

        public static readonly PdfName PRINTFIELD;

        public static readonly PdfName PRINTPAGERANGE;

        public static readonly PdfName PRINTSCALING;

        public static readonly PdfName PRINTSTATE;

        public static readonly PdfName PRIVATE;

        public static readonly PdfName PROCSET;

        public static readonly PdfName PRODUCER;

        public static readonly PdfName PROJCS;

        public static readonly PdfName PROP_BUILD;

        public static readonly PdfName PROPERTIES;

        public static readonly PdfName PS;

        public static readonly PdfName PTDATA;

        public static readonly PdfName PUBSEC;

        public static readonly PdfName PV;

        public static readonly PdfName Q;

        public static readonly PdfName QUADPOINTS;

        public static readonly PdfName QUOTE;

        public static readonly PdfName R;

        public static readonly PdfName R2L;

        public static readonly PdfName RANGE;

        public static readonly PdfName RB;

        public static readonly PdfName rb;

        public static readonly PdfName RBGROUPS;

        public static readonly PdfName RC;

        public static readonly PdfName RD;

        public static readonly PdfName REASON;

        public static readonly PdfName RECIPIENTS;

        public static readonly PdfName RECT;

        public static readonly PdfName REDACT;

        public static readonly PdfName REFERENCE;

        public static readonly PdfName REGISTRY;

        public static readonly PdfName REGISTRYNAME;

        public static readonly PdfName RELATIVECOLORIMETRIC;

        public static readonly PdfName RENDITION;

        public static readonly PdfName REPEAT;

        public static readonly PdfName RESETFORM;

        public static readonly PdfName RESOURCES;

        public static readonly PdfName REQUIREMENTS;

        public static readonly PdfName RI;

        public static readonly PdfName RICHMEDIA;

        public static readonly PdfName RICHMEDIAACTIVATION;

        public static readonly PdfName RICHMEDIAANIMATION;

        public static readonly PdfName RICHMEDIACOMMAND;

        public static readonly PdfName RICHMEDIACONFIGURATION;

        public static readonly PdfName RICHMEDIACONTENT;

        public static readonly PdfName RICHMEDIADEACTIVATION;

        public static readonly PdfName RICHMEDIAEXECUTE;

        public static readonly PdfName RICHMEDIAINSTANCE;

        public static readonly PdfName RICHMEDIAPARAMS;

        public static readonly PdfName RICHMEDIAPOSITION;

        public static readonly PdfName RICHMEDIAPRESENTATION;

        public static readonly PdfName RICHMEDIASETTINGS;

        public static readonly PdfName RICHMEDIAWINDOW;

        public static readonly PdfName RL;

        public static readonly PdfName ROLE;

        public static readonly PdfName RO;

        public static readonly PdfName ROLEMAP;

        public static readonly PdfName ROOT;

        public static readonly PdfName ROTATE;

        public static readonly PdfName ROW;

        public static readonly PdfName ROWS;

        public static readonly PdfName ROWSPAN;

        public static readonly PdfName RP;

        public static readonly PdfName RT;

        public static readonly PdfName RUBY;

        public static readonly PdfName RUNLENGTHDECODE;

        public static readonly PdfName RV;

        public static readonly PdfName S;

        public static readonly PdfName SATURATION;

        public static readonly PdfName SCHEMA;

        public static readonly PdfName SCOPE;

        public static readonly PdfName SCREEN;

        public static readonly PdfName SCRIPTS;

        public static readonly PdfName SECT;

        public static readonly PdfName SEPARATION;

        public static readonly PdfName SETOCGSTATE;

        public static readonly PdfName SETTINGS;

        public static readonly PdfName SHADING;

        public static readonly PdfName SHADINGTYPE;

        public static readonly PdfName SHIFT_JIS;

        public static readonly PdfName SIG;

        public static readonly PdfName SIGFIELDLOCK;

        public static readonly PdfName SIGFLAGS;

        public static readonly PdfName SIGREF;

        public static readonly PdfName SIMPLEX;

        public static readonly PdfName SINGLEPAGE;

        public static readonly PdfName SIZE;

        public static readonly PdfName SMASK;

        public static readonly PdfName SMASKINDATA;

        public static readonly PdfName SORT;

        public static readonly PdfName SOUND;

        public static readonly PdfName SPACEAFTER;

        public static readonly PdfName SPACEBEFORE;

        public static readonly PdfName SPAN;

        public static readonly PdfName SPEED;

        public static readonly PdfName SPLIT;

        public static readonly PdfName SQUARE;

        public static readonly PdfName SQUIGGLY;

        public static readonly PdfName SS;

        public static readonly PdfName ST;

        public static readonly PdfName STAMP;

        public static readonly PdfName STANDARD;

        public static readonly PdfName START;

        public static readonly PdfName STARTINDENT;

        public static readonly PdfName STATE;

        public static readonly PdfName STATUS;

        public static readonly PdfName STDCF;

        public static readonly PdfName STEMV;

        public static readonly PdfName STMF;

        public static readonly PdfName STRF;

        public static readonly PdfName STRIKEOUT;

        public static readonly PdfName STRUCTELEM;

        public static readonly PdfName STRUCTPARENT;

        public static readonly PdfName STRUCTPARENTS;

        public static readonly PdfName STRUCTTREEROOT;

        public static readonly PdfName STYLE;

        public static readonly PdfName SUBFILTER;

        public static readonly PdfName SUBJECT;

        public static readonly PdfName SUBMITFORM;

        public static readonly PdfName SUBTYPE;

        public static readonly PdfName SUMMARY;

        public static readonly PdfName SUPPLEMENT;

        public static readonly PdfName SV;

        public static readonly PdfName SW;

        public static readonly PdfName SYMBOL;

        public static readonly PdfName T;

        public static readonly PdfName TA;

        public static readonly PdfName TABLE;

        public static readonly PdfName TABS;

        public static readonly PdfName TBODY;

        public static readonly PdfName TD;

        public static readonly PdfName TR;

        public static readonly PdfName TR2;

        public static readonly PdfName TEXT;

        public static readonly PdfName TEXTALIGN;

        public static readonly PdfName TEXTDECORATIONCOLOR;

        public static readonly PdfName TEXTDECORATIONTHICKNESS;

        public static readonly PdfName TEXTDECORATIONTYPE;

        public static readonly PdfName TEXTINDENT;

        public static readonly PdfName TFOOT;

        public static readonly PdfName TH;

        public static readonly PdfName THEAD;

        public static readonly PdfName THUMB;

        public static readonly PdfName THREADS;

        public static readonly PdfName TI;

        public static readonly PdfName TIME;

        public static readonly PdfName TILINGTYPE;

        public static readonly PdfName TIMES_ROMAN;

        public static readonly PdfName TIMES_BOLD;

        public static readonly PdfName TIMES_ITALIC;

        public static readonly PdfName TIMES_BOLDITALIC;

        public static readonly PdfName TITLE;

        public static readonly PdfName TK;

        public static readonly PdfName TM;

        public static readonly PdfName TOC;

        public static readonly PdfName TOCI;

        public static readonly PdfName TOGGLE;

        public static readonly PdfName TOOLBAR;

        public static readonly PdfName TOUNICODE;

        public static readonly PdfName TP;

        public static readonly PdfName TABLEROW;

        public static readonly PdfName TRANS;

        public static readonly PdfName TRANSFORMPARAMS;

        public static readonly PdfName TRANSFORMMETHOD;

        public static readonly PdfName TRANSPARENCY;

        public static readonly PdfName TRANSPARENT;

        public static readonly PdfName TRAPNET;

        public static readonly PdfName TRAPPED;

        public static readonly PdfName TRIMBOX;

        public static readonly PdfName TRUETYPE;

        public static readonly PdfName TS;

        public static readonly PdfName TTL;

        public static readonly PdfName TU;

        public static readonly PdfName TV;

        public static readonly PdfName TWOCOLUMNLEFT;

        public static readonly PdfName TWOCOLUMNRIGHT;

        public static readonly PdfName TWOPAGELEFT;

        public static readonly PdfName TWOPAGERIGHT;

        public static readonly PdfName TX;

        public static readonly PdfName TYPE;

        public static readonly PdfName TYPE0;

        public static readonly PdfName TYPE1;

        public static readonly PdfName TYPE3;

        public static readonly PdfName U;

        public static readonly PdfName UE;

        public static readonly PdfName UF;

        public static readonly PdfName UHC;

        public static readonly PdfName UNDERLINE;

        public static readonly PdfName UNIX;

        public static readonly PdfName UPPERALPHA;

        public static readonly PdfName UPPERROMAN;

        public static readonly PdfName UR;

        public static readonly PdfName UR3;

        public static readonly PdfName URI;

        public static readonly PdfName URL;

        public static readonly PdfName USAGE;

        public static readonly PdfName USEATTACHMENTS;

        public static readonly PdfName USENONE;

        public static readonly PdfName USEOC;

        public static readonly PdfName USEOUTLINES;

        public static readonly PdfName USER;

        public static readonly PdfName USERPROPERTIES;

        public static readonly PdfName USERUNIT;

        public static readonly PdfName USETHUMBS;

        public static readonly PdfName UTF_8;

        public static readonly PdfName V;

        public static readonly PdfName V2;

        public static readonly PdfName VALIGN;

        public static readonly PdfName VE;

        public static readonly PdfName VERISIGN_PPKVS;

        public static readonly PdfName VERSION;

        public static readonly PdfName VERTICES;

        public static readonly PdfName VIDEO;

        public static readonly PdfName VIEW;

        public static readonly PdfName VIEWS;

        public static readonly PdfName VIEWAREA;

        public static readonly PdfName VIEWCLIP;

        public static readonly PdfName VIEWERPREFERENCES;

        public static readonly PdfName VIEWPORT;

        public static readonly PdfName VIEWSTATE;

        public static readonly PdfName VISIBLEPAGES;

        public static readonly PdfName VOFFSET;

        public static readonly PdfName VP;

        public static readonly PdfName VRI;

        public static readonly PdfName W;

        public static readonly PdfName W2;

        public static readonly PdfName WARICHU;

        public static readonly PdfName WATERMARK;

        public static readonly PdfName WC;

        public static readonly PdfName WIDGET;

        public static readonly PdfName WIDTH;

        public static readonly PdfName WIDTHS;

        public static readonly PdfName WIN;

        public static readonly PdfName WIN_ANSI_ENCODING;

        public static readonly PdfName WINDOW;

        public static readonly PdfName WINDOWED;

        public static readonly PdfName WIPE;

        public static readonly PdfName WHITEPOINT;

        public static readonly PdfName WKT;

        public static readonly PdfName WP;

        public static readonly PdfName WS;

        public static readonly PdfName WT;

        public static readonly PdfName X;

        public static readonly PdfName XA;

        public static readonly PdfName XD;

        public static readonly PdfName XFA;

        public static readonly PdfName XML;

        public static readonly PdfName XOBJECT;

        public static readonly PdfName XPTS;

        public static readonly PdfName XREF;

        public static readonly PdfName XREFSTM;

        public static readonly PdfName XSTEP;

        public static readonly PdfName XYZ;

        public static readonly PdfName YSTEP;

        public static readonly PdfName ZADB;

        public static readonly PdfName ZAPFDINGBATS;

        public static readonly PdfName ZOOM;

        public static Dictionary<string, PdfName> staticNames;

        private int hash;

        static PdfName()
        {
            _3D = new PdfName("3D");
            A = new PdfName("A");
            A85 = new PdfName("A85");
            AA = new PdfName("AA");
            ABSOLUTECOLORIMETRIC = new PdfName("AbsoluteColorimetric");
            AC = new PdfName("AC");
            ACROFORM = new PdfName("AcroForm");
            ACTION = new PdfName("Action");
            ACTIVATION = new PdfName("Activation");
            ADBE = new PdfName("ADBE");
            ACTUALTEXT = new PdfName("ActualText");
            ADBE_PKCS7_DETACHED = new PdfName("adbe.pkcs7.detached");
            ADBE_PKCS7_S4 = new PdfName("adbe.pkcs7.s4");
            ADBE_PKCS7_S5 = new PdfName("adbe.pkcs7.s5");
            ADBE_PKCS7_SHA1 = new PdfName("adbe.pkcs7.sha1");
            ADBE_X509_RSA_SHA1 = new PdfName("adbe.x509.rsa_sha1");
            ADOBE_PPKLITE = new PdfName("Adobe.PPKLite");
            ADOBE_PPKMS = new PdfName("Adobe.PPKMS");
            AESV2 = new PdfName("AESV2");
            AESV3 = new PdfName("AESV3");
            AFRELATIONSHIP = new PdfName("AFRelationship");
            AHX = new PdfName("AHx");
            AIS = new PdfName("AIS");
            ALL = new PdfName("All");
            ALLPAGES = new PdfName("AllPages");
            ALT = new PdfName("Alt");
            ALTERNATE = new PdfName("Alternate");
            ALTERNATEPRESENTATION = new PdfName("AlternatePresentations");
            ALTERNATES = new PdfName("Alternates");
            AND = new PdfName("And");
            ANIMATION = new PdfName("Animation");
            ANNOT = new PdfName("Annot");
            ANNOTS = new PdfName("Annots");
            ANTIALIAS = new PdfName("AntiAlias");
            AP = new PdfName("AP");
            APP = new PdfName("App");
            APPDEFAULT = new PdfName("AppDefault");
            ART = new PdfName("Art");
            ARTBOX = new PdfName("ArtBox");
            ARTIFACT = new PdfName("Artifact");
            ASCENT = new PdfName("Ascent");
            AS = new PdfName("AS");
            ASCII85DECODE = new PdfName("ASCII85Decode");
            ASCIIHEXDECODE = new PdfName("ASCIIHexDecode");
            ASSET = new PdfName("Asset");
            ASSETS = new PdfName("Assets");
            ATTACHED = new PdfName("Attached");
            AUTHEVENT = new PdfName("AuthEvent");
            AUTHOR = new PdfName("Author");
            B = new PdfName("B");
            BACKGROUND = new PdfName("Background");
            BACKGROUNDCOLOR = new PdfName("BackgroundColor");
            BASEENCODING = new PdfName("BaseEncoding");
            BASEFONT = new PdfName("BaseFont");
            BASEVERSION = new PdfName("BaseVersion");
            BBOX = new PdfName("BBox");
            BC = new PdfName("BC");
            BG = new PdfName("BG");
            BIBENTRY = new PdfName("BibEntry");
            BIGFIVE = new PdfName("BigFive");
            BINDING = new PdfName("Binding");
            BINDINGMATERIALNAME = new PdfName("BindingMaterialName");
            BITSPERCOMPONENT = new PdfName("BitsPerComponent");
            BITSPERSAMPLE = new PdfName("BitsPerSample");
            BL = new PdfName("Bl");
            BLACKIS1 = new PdfName("BlackIs1");
            BLACKPOINT = new PdfName("BlackPoint");
            BLOCKQUOTE = new PdfName("BlockQuote");
            BLEEDBOX = new PdfName("BleedBox");
            BLINDS = new PdfName("Blinds");
            BM = new PdfName("BM");
            BORDER = new PdfName("Border");
            BOTH = new PdfName("Both");
            BOUNDS = new PdfName("Bounds");
            BOX = new PdfName("Box");
            BS = new PdfName("BS");
            BTN = new PdfName("Btn");
            BYTERANGE = new PdfName("ByteRange");
            C = new PdfName("C");
            C0 = new PdfName("C0");
            C1 = new PdfName("C1");
            CA = new PdfName("CA");
            ca = new PdfName("ca");
            CALGRAY = new PdfName("CalGray");
            CALRGB = new PdfName("CalRGB");
            CAPHEIGHT = new PdfName("CapHeight");
            CARET = new PdfName("Caret");
            CAPTION = new PdfName("Caption");
            CATALOG = new PdfName("Catalog");
            CATEGORY = new PdfName("Category");
            CB = new PdfName("cb");
            CCITTFAXDECODE = new PdfName("CCITTFaxDecode");
            CENTER = new PdfName("Center");
            CENTERWINDOW = new PdfName("CenterWindow");
            CERT = new PdfName("Cert");
            CERTS = new PdfName("Certs");
            CF = new PdfName("CF");
            CFM = new PdfName("CFM");
            CH = new PdfName("Ch");
            CHARPROCS = new PdfName("CharProcs");
            CHECKSUM = new PdfName("CheckSum");
            CI = new PdfName("CI");
            CIDFONTTYPE0 = new PdfName("CIDFontType0");
            CIDFONTTYPE2 = new PdfName("CIDFontType2");
            CIDSET = new PdfName("CIDSet");
            CIDSYSTEMINFO = new PdfName("CIDSystemInfo");
            CIDTOGIDMAP = new PdfName("CIDToGIDMap");
            CIRCLE = new PdfName("Circle");
            CLASSMAP = new PdfName("ClassMap");
            CLOUD = new PdfName("Cloud");
            CMD = new PdfName("CMD");
            CO = new PdfName("CO");
            CODE = new PdfName("Code");
            COLOR = new PdfName("Color");
            COLORANTS = new PdfName("Colorants");
            COLORS = new PdfName("Colors");
            COLORSPACE = new PdfName("ColorSpace");
            COLORTRANSFORM = new PdfName("ColorTransform");
            COLLECTION = new PdfName("Collection");
            COLLECTIONFIELD = new PdfName("CollectionField");
            COLLECTIONITEM = new PdfName("CollectionItem");
            COLLECTIONSCHEMA = new PdfName("CollectionSchema");
            COLLECTIONSORT = new PdfName("CollectionSort");
            COLLECTIONSUBITEM = new PdfName("CollectionSubitem");
            COLSPAN = new PdfName("ColSpan");
            COLUMN = new PdfName("Column");
            COLUMNS = new PdfName("Columns");
            CONDITION = new PdfName("Condition");
            CONFIGS = new PdfName("Configs");
            CONFIGURATION = new PdfName("Configuration");
            CONFIGURATIONS = new PdfName("Configurations");
            CONTACTINFO = new PdfName("ContactInfo");
            CONTENT = new PdfName("Content");
            CONTENTS = new PdfName("Contents");
            COORDS = new PdfName("Coords");
            COUNT = new PdfName("Count");
            COURIER = new PdfName("Courier");
            COURIER_BOLD = new PdfName("Courier-Bold");
            COURIER_OBLIQUE = new PdfName("Courier-Oblique");
            COURIER_BOLDOBLIQUE = new PdfName("Courier-BoldOblique");
            CREATIONDATE = new PdfName("CreationDate");
            CREATOR = new PdfName("Creator");
            CREATORINFO = new PdfName("CreatorInfo");
            CRL = new PdfName("CRL");
            CRLS = new PdfName("CRLs");
            CROPBOX = new PdfName("CropBox");
            CRYPT = new PdfName("Crypt");
            CS = new PdfName("CS");
            CUEPOINT = new PdfName("CuePoint");
            CUEPOINTS = new PdfName("CuePoints");
            CYX = new PdfName("CYX");
            D = new PdfName("D");
            DA = new PdfName("DA");
            DATA = new PdfName("Data");
            DC = new PdfName("DC");
            DCS = new PdfName("DCS");
            DCTDECODE = new PdfName("DCTDecode");
            DECIMAL = new PdfName("Decimal");
            DEACTIVATION = new PdfName("Deactivation");
            DECODE = new PdfName("Decode");
            DECODEPARMS = new PdfName("DecodeParms");
            DEFAULT = new PdfName("Default");
            DEFAULTCRYPTFILTER = new PdfName("DefaultCryptFilter");
            DEFAULTCMYK = new PdfName("DefaultCMYK");
            DEFAULTGRAY = new PdfName("DefaultGray");
            DEFAULTRGB = new PdfName("DefaultRGB");
            DESC = new PdfName("Desc");
            DESCENDANTFONTS = new PdfName("DescendantFonts");
            DESCENT = new PdfName("Descent");
            DEST = new PdfName("Dest");
            DESTOUTPUTPROFILE = new PdfName("DestOutputProfile");
            DESTS = new PdfName("Dests");
            DEVICEGRAY = new PdfName("DeviceGray");
            DEVICERGB = new PdfName("DeviceRGB");
            DEVICECMYK = new PdfName("DeviceCMYK");
            DEVICEN = new PdfName("DeviceN");
            DI = new PdfName("Di");
            DIFFERENCES = new PdfName("Differences");
            DISSOLVE = new PdfName("Dissolve");
            DIRECTION = new PdfName("Direction");
            DISPLAYDOCTITLE = new PdfName("DisplayDocTitle");
            DIV = new PdfName("Div");
            DL = new PdfName("DL");
            DM = new PdfName("Dm");
            DOS = new PdfName("DOS");
            DOCMDP = new PdfName("DocMDP");
            DOCOPEN = new PdfName("DocOpen");
            DOCTIMESTAMP = new PdfName("DocTimeStamp");
            DOCUMENT = new PdfName("Document");
            DOMAIN = new PdfName("Domain");
            DP = new PdfName("DP");
            DR = new PdfName("DR");
            DS = new PdfName("DS");
            DSS = new PdfName("DSS");
            DUR = new PdfName("Dur");
            DUPLEX = new PdfName("Duplex");
            DUPLEXFLIPSHORTEDGE = new PdfName("DuplexFlipShortEdge");
            DUPLEXFLIPLONGEDGE = new PdfName("DuplexFlipLongEdge");
            DV = new PdfName("DV");
            DW = new PdfName("DW");
            E = new PdfName("E");
            EARLYCHANGE = new PdfName("EarlyChange");
            EF = new PdfName("EF");
            EFF = new PdfName("EFF");
            EFOPEN = new PdfName("EFOpen");
            EMBEDDED = new PdfName("Embedded");
            EMBEDDEDFILE = new PdfName("EmbeddedFile");
            EMBEDDEDFILES = new PdfName("EmbeddedFiles");
            ENCODE = new PdfName("Encode");
            ENCODEDBYTEALIGN = new PdfName("EncodedByteAlign");
            ENCODING = new PdfName("Encoding");
            ENCRYPT = new PdfName("Encrypt");
            ENCRYPTMETADATA = new PdfName("EncryptMetadata");
            END = new PdfName("End");
            ENDINDENT = new PdfName("EndIndent");
            ENDOFBLOCK = new PdfName("EndOfBlock");
            ENDOFLINE = new PdfName("EndOfLine");
            EPSG = new PdfName("EPSG");
            ESIC = new PdfName("ESIC");
            ETSI_CADES_DETACHED = new PdfName("ETSI.CAdES.detached");
            ETSI_RFC3161 = new PdfName("ETSI.RFC3161");
            EXCLUDE = new PdfName("Exclude");
            EXTEND = new PdfName("Extend");
            EXTENSIONS = new PdfName("Extensions");
            EXTENSIONLEVEL = new PdfName("ExtensionLevel");
            EXTGSTATE = new PdfName("ExtGState");
            EXPORT = new PdfName("Export");
            EXPORTSTATE = new PdfName("ExportState");
            EVENT = new PdfName("Event");
            F = new PdfName("F");
            FAR = new PdfName("Far");
            FB = new PdfName("FB");
            FD = new PdfName("FD");
            FDECODEPARMS = new PdfName("FDecodeParms");
            FDF = new PdfName("FDF");
            FF = new PdfName("Ff");
            FFILTER = new PdfName("FFilter");
            FG = new PdfName("FG");
            FIELDMDP = new PdfName("FieldMDP");
            FIELDS = new PdfName("Fields");
            FIGURE = new PdfName("Figure");
            FILEATTACHMENT = new PdfName("FileAttachment");
            FILESPEC = new PdfName("Filespec");
            FILTER = new PdfName("Filter");
            FIRST = new PdfName("First");
            FIRSTCHAR = new PdfName("FirstChar");
            FIRSTPAGE = new PdfName("FirstPage");
            FIT = new PdfName("Fit");
            FITH = new PdfName("FitH");
            FITV = new PdfName("FitV");
            FITR = new PdfName("FitR");
            FITB = new PdfName("FitB");
            FITBH = new PdfName("FitBH");
            FITBV = new PdfName("FitBV");
            FITWINDOW = new PdfName("FitWindow");
            FL = new PdfName("Fl");
            FLAGS = new PdfName("Flags");
            FLASH = new PdfName("Flash");
            FLASHVARS = new PdfName("FlashVars");
            FLATEDECODE = new PdfName("FlateDecode");
            FO = new PdfName("Fo");
            FONT = new PdfName("Font");
            FONTBBOX = new PdfName("FontBBox");
            FONTDESCRIPTOR = new PdfName("FontDescriptor");
            FONTFAMILY = new PdfName("FontFamily");
            FONTFILE = new PdfName("FontFile");
            FONTFILE2 = new PdfName("FontFile2");
            FONTFILE3 = new PdfName("FontFile3");
            FONTMATRIX = new PdfName("FontMatrix");
            FONTNAME = new PdfName("FontName");
            FONTWEIGHT = new PdfName("FontWeight");
            FOREGROUND = new PdfName("Foreground");
            FORM = new PdfName("Form");
            FORMTYPE = new PdfName("FormType");
            FORMULA = new PdfName("Formula");
            FREETEXT = new PdfName("FreeText");
            FRM = new PdfName("FRM");
            FS = new PdfName("FS");
            FT = new PdfName("FT");
            FULLSCREEN = new PdfName("FullScreen");
            FUNCTION = new PdfName("Function");
            FUNCTIONS = new PdfName("Functions");
            FUNCTIONTYPE = new PdfName("FunctionType");
            GAMMA = new PdfName("Gamma");
            GBK = new PdfName("GBK");
            GCS = new PdfName("GCS");
            GEO = new PdfName("GEO");
            GEOGCS = new PdfName("GEOGCS");
            GLITTER = new PdfName("Glitter");
            GOTO = new PdfName("GoTo");
            GOTO3DVIEW = new PdfName("GoTo3DView");
            GOTOE = new PdfName("GoToE");
            GOTOR = new PdfName("GoToR");
            GPTS = new PdfName("GPTS");
            GROUP = new PdfName("Group");
            GTS_PDFA1 = new PdfName("GTS_PDFA1");
            GTS_PDFX = new PdfName("GTS_PDFX");
            GTS_PDFXVERSION = new PdfName("GTS_PDFXVersion");
            H = new PdfName("H");
            H1 = new PdfName("H1");
            H2 = new PdfName("H2");
            H3 = new PdfName("H3");
            H4 = new PdfName("H4");
            H5 = new PdfName("H5");
            H6 = new PdfName("H6");
            HALFTONENAME = new PdfName("HalftoneName");
            HALFTONETYPE = new PdfName("HalftoneType");
            HALIGN = new PdfName("HAlign");
            HEADERS = new PdfName("Headers");
            HEIGHT = new PdfName("Height");
            HELV = new PdfName("Helv");
            HELVETICA = new PdfName("Helvetica");
            HELVETICA_BOLD = new PdfName("Helvetica-Bold");
            HELVETICA_OBLIQUE = new PdfName("Helvetica-Oblique");
            HELVETICA_BOLDOBLIQUE = new PdfName("Helvetica-BoldOblique");
            HF = new PdfName("HF");
            HID = new PdfName("Hid");
            HIDE = new PdfName("Hide");
            HIDEMENUBAR = new PdfName("HideMenubar");
            HIDETOOLBAR = new PdfName("HideToolbar");
            HIDEWINDOWUI = new PdfName("HideWindowUI");
            HIGHLIGHT = new PdfName("Highlight");
            HOFFSET = new PdfName("HOffset");
            HT = new PdfName("HT");
            HTP = new PdfName("HTP");
            I = new PdfName("I");
            IC = new PdfName("IC");
            ICCBASED = new PdfName("ICCBased");
            ID = new PdfName("ID");
            IDENTITY = new PdfName("Identity");
            IF = new PdfName("IF");
            IM = new PdfName("IM");
            IMAGE = new PdfName("Image");
            IMAGEB = new PdfName("ImageB");
            IMAGEC = new PdfName("ImageC");
            IMAGEI = new PdfName("ImageI");
            IMAGEMASK = new PdfName("ImageMask");
            INCLUDE = new PdfName("Include");
            IND = new PdfName("Ind");
            INDEX = new PdfName("Index");
            INDEXED = new PdfName("Indexed");
            INFO = new PdfName("Info");
            INK = new PdfName("Ink");
            INKLIST = new PdfName("InkList");
            INSTANCES = new PdfName("Instances");
            IMPORTDATA = new PdfName("ImportData");
            INTENT = new PdfName("Intent");
            INTERPOLATE = new PdfName("Interpolate");
            ISMAP = new PdfName("IsMap");
            IRT = new PdfName("IRT");
            ITALICANGLE = new PdfName("ItalicAngle");
            ITXT = new PdfName("ITXT");
            IX = new PdfName("IX");
            JAVASCRIPT = new PdfName("JavaScript");
            JBIG2DECODE = new PdfName("JBIG2Decode");
            JBIG2GLOBALS = new PdfName("JBIG2Globals");
            JPXDECODE = new PdfName("JPXDecode");
            JS = new PdfName("JS");
            JUSTIFY = new PdfName("Justify");
            K = new PdfName("K");
            KEYWORDS = new PdfName("Keywords");
            KIDS = new PdfName("Kids");
            L = new PdfName("L");
            L2R = new PdfName("L2R");
            LAB = new PdfName("Lab");
            LANG = new PdfName("Lang");
            LANGUAGE = new PdfName("Language");
            LAST = new PdfName("Last");
            LASTCHAR = new PdfName("LastChar");
            LASTPAGE = new PdfName("LastPage");
            LAUNCH = new PdfName("Launch");
            LAYOUT = new PdfName("Layout");
            LBL = new PdfName("Lbl");
            LBODY = new PdfName("LBody");
            LENGTH = new PdfName("Length");
            LENGTH1 = new PdfName("Length1");
            LI = new PdfName("LI");
            LIMITS = new PdfName("Limits");
            LINE = new PdfName("Line");
            LINEAR = new PdfName("Linear");
            LINEHEIGHT = new PdfName("LineHeight");
            LINK = new PdfName("Link");
            LIST = new PdfName("List");
            LISTMODE = new PdfName("ListMode");
            LISTNUMBERING = new PdfName("ListNumbering");
            LOCATION = new PdfName("Location");
            LOCK = new PdfName("Lock");
            LOCKED = new PdfName("Locked");
            LOWERALPHA = new PdfName("LowerAlpha");
            LOWERROMAN = new PdfName("LowerRoman");
            LPTS = new PdfName("LPTS");
            LZWDECODE = new PdfName("LZWDecode");
            M = new PdfName("M");
            MAC = new PdfName("Mac");
            MATERIAL = new PdfName("Material");
            MATRIX = new PdfName("Matrix");
            MAC_EXPERT_ENCODING = new PdfName("MacExpertEncoding");
            MAC_ROMAN_ENCODING = new PdfName("MacRomanEncoding");
            MARKED = new PdfName("Marked");
            MARKINFO = new PdfName("MarkInfo");
            MASK = new PdfName("Mask");
            MAX_LOWER_CASE = new PdfName("max");
            MAX_CAMEL_CASE = new PdfName("Max");
            MAXLEN = new PdfName("MaxLen");
            MEDIABOX = new PdfName("MediaBox");
            MCID = new PdfName("MCID");
            MCR = new PdfName("MCR");
            MEASURE = new PdfName("Measure");
            METADATA = new PdfName("Metadata");
            MIN_LOWER_CASE = new PdfName("min");
            MIN_CAMEL_CASE = new PdfName("Min");
            MK = new PdfName("MK");
            MMTYPE1 = new PdfName("MMType1");
            MODDATE = new PdfName("ModDate");
            MOVIE = new PdfName("Movie");
            N = new PdfName("N");
            N0 = new PdfName("n0");
            N1 = new PdfName("n1");
            N2 = new PdfName("n2");
            N3 = new PdfName("n3");
            N4 = new PdfName("n4");
            NAME = new PdfName("Name");
            NAMED = new PdfName("Named");
            NAMES = new PdfName("Names");
            NAVIGATION = new PdfName("Navigation");
            NAVIGATIONPANE = new PdfName("NavigationPane");
            NCHANNEL = new PdfName("NChannel");
            NEAR = new PdfName("Near");
            NEEDAPPEARANCES = new PdfName("NeedAppearances");
            NEEDRENDERING = new PdfName("NeedsRendering");
            NEWWINDOW = new PdfName("NewWindow");
            NEXT = new PdfName("Next");
            NEXTPAGE = new PdfName("NextPage");
            NM = new PdfName("NM");
            NONE = new PdfName("None");
            NONFULLSCREENPAGEMODE = new PdfName("NonFullScreenPageMode");
            NONSTRUCT = new PdfName("NonStruct");
            NOT = new PdfName("Not");
            NOTE = new PdfName("Note");
            NUMBERFORMAT = new PdfName("NumberFormat");
            NUMCOPIES = new PdfName("NumCopies");
            NUMS = new PdfName("Nums");
            O = new PdfName("O");
            OBJ = new PdfName("Obj");
            OBJR = new PdfName("OBJR");
            OBJSTM = new PdfName("ObjStm");
            OC = new PdfName("OC");
            OCG = new PdfName("OCG");
            OCGS = new PdfName("OCGs");
            OCMD = new PdfName("OCMD");
            OCPROPERTIES = new PdfName("OCProperties");
            OCSP = new PdfName("OCSP");
            OCSPS = new PdfName("OCSPs");
            OE = new PdfName("OE");
            Off_ = new PdfName("Off");
            OFF = new PdfName("OFF");
            ON = new PdfName("ON");
            ONECOLUMN = new PdfName("OneColumn");
            OPEN = new PdfName("Open");
            OPENACTION = new PdfName("OpenAction");
            OP = new PdfName("OP");
            op_ = new PdfName("op");
            OPI = new PdfName("OPI");
            OPM = new PdfName("OPM");
            OPT = new PdfName("Opt");
            OR = new PdfName("Or");
            ORDER = new PdfName("Order");
            ORDERING = new PdfName("Ordering");
            ORG = new PdfName("Org");
            OSCILLATING = new PdfName("Oscillating");
            OUTLINES = new PdfName("Outlines");
            OUTPUTCONDITION = new PdfName("OutputCondition");
            OUTPUTCONDITIONIDENTIFIER = new PdfName("OutputConditionIdentifier");
            OUTPUTINTENT = new PdfName("OutputIntent");
            OUTPUTINTENTS = new PdfName("OutputIntents");
            OVERLAYTEXT = new PdfName("OverlayText");
            P = new PdfName("P");
            PAGE = new PdfName("Page");
            PAGEELEMENT = new PdfName("PageElement");
            PAGELABELS = new PdfName("PageLabels");
            PAGELAYOUT = new PdfName("PageLayout");
            PAGEMODE = new PdfName("PageMode");
            PAGES = new PdfName("Pages");
            PAINTTYPE = new PdfName("PaintType");
            PANOSE = new PdfName("Panose");
            PARAMS = new PdfName("Params");
            PARENT = new PdfName("Parent");
            PARENTTREE = new PdfName("ParentTree");
            PARENTTREENEXTKEY = new PdfName("ParentTreeNextKey");
            PART = new PdfName("Part");
            PASSCONTEXTCLICK = new PdfName("PassContextClick");
            PATTERN = new PdfName("Pattern");
            PATTERNTYPE = new PdfName("PatternType");
            PB = new PdfName("pb");
            PC = new PdfName("PC");
            PDF = new PdfName("PDF");
            PDFDOCENCODING = new PdfName("PDFDocEncoding");
            PDU = new PdfName("PDU");
            PERCEPTUAL = new PdfName("Perceptual");
            PERMS = new PdfName("Perms");
            PG = new PdfName("Pg");
            PI = new PdfName("PI");
            PICKTRAYBYPDFSIZE = new PdfName("PickTrayByPDFSize");
            PIECEINFO = new PdfName("PieceInfo");
            PLAYCOUNT = new PdfName("PlayCount");
            PO = new PdfName("PO");
            POLYGON = new PdfName("Polygon");
            POLYLINE = new PdfName("PolyLine");
            POPUP = new PdfName("Popup");
            POSITION = new PdfName("Position");
            PREDICTOR = new PdfName("Predictor");
            PREFERRED = new PdfName("Preferred");
            PRESENTATION = new PdfName("Presentation");
            PRESERVERB = new PdfName("PreserveRB");
            PRESSTEPS = new PdfName("PresSteps");
            PREV = new PdfName("Prev");
            PREVPAGE = new PdfName("PrevPage");
            PRINT = new PdfName("Print");
            PRINTAREA = new PdfName("PrintArea");
            PRINTCLIP = new PdfName("PrintClip");
            PRINTERMARK = new PdfName("PrinterMark");
            PRINTFIELD = new PdfName("PrintField");
            PRINTPAGERANGE = new PdfName("PrintPageRange");
            PRINTSCALING = new PdfName("PrintScaling");
            PRINTSTATE = new PdfName("PrintState");
            PRIVATE = new PdfName("Private");
            PROCSET = new PdfName("ProcSet");
            PRODUCER = new PdfName("Producer");
            PROJCS = new PdfName("PROJCS");
            PROP_BUILD = new PdfName("Prop_Build");
            PROPERTIES = new PdfName("Properties");
            PS = new PdfName("PS");
            PTDATA = new PdfName("PtData");
            PUBSEC = new PdfName("Adobe.PubSec");
            PV = new PdfName("PV");
            Q = new PdfName("Q");
            QUADPOINTS = new PdfName("QuadPoints");
            QUOTE = new PdfName("Quote");
            R = new PdfName("R");
            R2L = new PdfName("R2L");
            RANGE = new PdfName("Range");
            RB = new PdfName("RB");
            rb = new PdfName("rb");
            RBGROUPS = new PdfName("RBGroups");
            RC = new PdfName("RC");
            RD = new PdfName("RD");
            REASON = new PdfName("Reason");
            RECIPIENTS = new PdfName("Recipients");
            RECT = new PdfName("Rect");
            REDACT = new PdfName("Redact");
            REFERENCE = new PdfName("Reference");
            REGISTRY = new PdfName("Registry");
            REGISTRYNAME = new PdfName("RegistryName");
            RELATIVECOLORIMETRIC = new PdfName("RelativeColorimetric");
            RENDITION = new PdfName("Rendition");
            REPEAT = new PdfName("Repeat");
            RESETFORM = new PdfName("ResetForm");
            RESOURCES = new PdfName("Resources");
            REQUIREMENTS = new PdfName("Requirements");
            RI = new PdfName("RI");
            RICHMEDIA = new PdfName("RichMedia");
            RICHMEDIAACTIVATION = new PdfName("RichMediaActivation");
            RICHMEDIAANIMATION = new PdfName("RichMediaAnimation");
            RICHMEDIACOMMAND = new PdfName("RichMediaCommand");
            RICHMEDIACONFIGURATION = new PdfName("RichMediaConfiguration");
            RICHMEDIACONTENT = new PdfName("RichMediaContent");
            RICHMEDIADEACTIVATION = new PdfName("RichMediaDeactivation");
            RICHMEDIAEXECUTE = new PdfName("RichMediaExecute");
            RICHMEDIAINSTANCE = new PdfName("RichMediaInstance");
            RICHMEDIAPARAMS = new PdfName("RichMediaParams");
            RICHMEDIAPOSITION = new PdfName("RichMediaPosition");
            RICHMEDIAPRESENTATION = new PdfName("RichMediaPresentation");
            RICHMEDIASETTINGS = new PdfName("RichMediaSettings");
            RICHMEDIAWINDOW = new PdfName("RichMediaWindow");
            RL = new PdfName("RL");
            ROLE = new PdfName("Role");
            RO = new PdfName("RO");
            ROLEMAP = new PdfName("RoleMap");
            ROOT = new PdfName("Root");
            ROTATE = new PdfName("Rotate");
            ROW = new PdfName("Row");
            ROWS = new PdfName("Rows");
            ROWSPAN = new PdfName("RowSpan");
            RP = new PdfName("RP");
            RT = new PdfName("RT");
            RUBY = new PdfName("Ruby");
            RUNLENGTHDECODE = new PdfName("RunLengthDecode");
            RV = new PdfName("RV");
            S = new PdfName("S");
            SATURATION = new PdfName("Saturation");
            SCHEMA = new PdfName("Schema");
            SCOPE = new PdfName("Scope");
            SCREEN = new PdfName("Screen");
            SCRIPTS = new PdfName("Scripts");
            SECT = new PdfName("Sect");
            SEPARATION = new PdfName("Separation");
            SETOCGSTATE = new PdfName("SetOCGState");
            SETTINGS = new PdfName("Settings");
            SHADING = new PdfName("Shading");
            SHADINGTYPE = new PdfName("ShadingType");
            SHIFT_JIS = new PdfName("Shift-JIS");
            SIG = new PdfName("Sig");
            SIGFIELDLOCK = new PdfName("SigFieldLock");
            SIGFLAGS = new PdfName("SigFlags");
            SIGREF = new PdfName("SigRef");
            SIMPLEX = new PdfName("Simplex");
            SINGLEPAGE = new PdfName("SinglePage");
            SIZE = new PdfName("Size");
            SMASK = new PdfName("SMask");
            SMASKINDATA = new PdfName("SMaskInData");
            SORT = new PdfName("Sort");
            SOUND = new PdfName("Sound");
            SPACEAFTER = new PdfName("SpaceAfter");
            SPACEBEFORE = new PdfName("SpaceBefore");
            SPAN = new PdfName("Span");
            SPEED = new PdfName("Speed");
            SPLIT = new PdfName("Split");
            SQUARE = new PdfName("Square");
            SQUIGGLY = new PdfName("Squiggly");
            SS = new PdfName("SS");
            ST = new PdfName("St");
            STAMP = new PdfName("Stamp");
            STANDARD = new PdfName("Standard");
            START = new PdfName("Start");
            STARTINDENT = new PdfName("StartIndent");
            STATE = new PdfName("State");
            STATUS = new PdfName("Status");
            STDCF = new PdfName("StdCF");
            STEMV = new PdfName("StemV");
            STMF = new PdfName("StmF");
            STRF = new PdfName("StrF");
            STRIKEOUT = new PdfName("StrikeOut");
            STRUCTELEM = new PdfName("StructElem");
            STRUCTPARENT = new PdfName("StructParent");
            STRUCTPARENTS = new PdfName("StructParents");
            STRUCTTREEROOT = new PdfName("StructTreeRoot");
            STYLE = new PdfName("Style");
            SUBFILTER = new PdfName("SubFilter");
            SUBJECT = new PdfName("Subject");
            SUBMITFORM = new PdfName("SubmitForm");
            SUBTYPE = new PdfName("Subtype");
            SUMMARY = new PdfName("Summary");
            SUPPLEMENT = new PdfName("Supplement");
            SV = new PdfName("SV");
            SW = new PdfName("SW");
            SYMBOL = new PdfName("Symbol");
            T = new PdfName("T");
            TA = new PdfName("TA");
            TABLE = new PdfName("Table");
            TABS = new PdfName("Tabs");
            TBODY = new PdfName("TBody");
            TD = new PdfName("TD");
            TR = new PdfName("TR");
            TR2 = new PdfName("TR2");
            TEXT = new PdfName("Text");
            TEXTALIGN = new PdfName("TextAlign");
            TEXTDECORATIONCOLOR = new PdfName("TextDecorationColor");
            TEXTDECORATIONTHICKNESS = new PdfName("TextDecorationThickness");
            TEXTDECORATIONTYPE = new PdfName("TextDecorationType");
            TEXTINDENT = new PdfName("TextIndent");
            TFOOT = new PdfName("TFoot");
            TH = new PdfName("TH");
            THEAD = new PdfName("THead");
            THUMB = new PdfName("Thumb");
            THREADS = new PdfName("Threads");
            TI = new PdfName("TI");
            TIME = new PdfName("Time");
            TILINGTYPE = new PdfName("TilingType");
            TIMES_ROMAN = new PdfName("Times-Roman");
            TIMES_BOLD = new PdfName("Times-Bold");
            TIMES_ITALIC = new PdfName("Times-Italic");
            TIMES_BOLDITALIC = new PdfName("Times-BoldItalic");
            TITLE = new PdfName("Title");
            TK = new PdfName("TK");
            TM = new PdfName("TM");
            TOC = new PdfName("TOC");
            TOCI = new PdfName("TOCI");
            TOGGLE = new PdfName("Toggle");
            TOOLBAR = new PdfName("Toolbar");
            TOUNICODE = new PdfName("ToUnicode");
            TP = new PdfName("TP");
            TABLEROW = new PdfName("TR");
            TRANS = new PdfName("Trans");
            TRANSFORMPARAMS = new PdfName("TransformParams");
            TRANSFORMMETHOD = new PdfName("TransformMethod");
            TRANSPARENCY = new PdfName("Transparency");
            TRANSPARENT = new PdfName("Transparent");
            TRAPNET = new PdfName("TrapNet");
            TRAPPED = new PdfName("Trapped");
            TRIMBOX = new PdfName("TrimBox");
            TRUETYPE = new PdfName("TrueType");
            TS = new PdfName("TS");
            TTL = new PdfName("Ttl");
            TU = new PdfName("TU");
            TV = new PdfName("tv");
            TWOCOLUMNLEFT = new PdfName("TwoColumnLeft");
            TWOCOLUMNRIGHT = new PdfName("TwoColumnRight");
            TWOPAGELEFT = new PdfName("TwoPageLeft");
            TWOPAGERIGHT = new PdfName("TwoPageRight");
            TX = new PdfName("Tx");
            TYPE = new PdfName("Type");
            TYPE0 = new PdfName("Type0");
            TYPE1 = new PdfName("Type1");
            TYPE3 = new PdfName("Type3");
            U = new PdfName("U");
            UE = new PdfName("UE");
            UF = new PdfName("UF");
            UHC = new PdfName("UHC");
            UNDERLINE = new PdfName("Underline");
            UNIX = new PdfName("Unix");
            UPPERALPHA = new PdfName("UpperAlpha");
            UPPERROMAN = new PdfName("UpperRoman");
            UR = new PdfName("UR");
            UR3 = new PdfName("UR3");
            URI = new PdfName("URI");
            URL = new PdfName("URL");
            USAGE = new PdfName("Usage");
            USEATTACHMENTS = new PdfName("UseAttachments");
            USENONE = new PdfName("UseNone");
            USEOC = new PdfName("UseOC");
            USEOUTLINES = new PdfName("UseOutlines");
            USER = new PdfName("User");
            USERPROPERTIES = new PdfName("UserProperties");
            USERUNIT = new PdfName("UserUnit");
            USETHUMBS = new PdfName("UseThumbs");
            UTF_8 = new PdfName("utf_8");
            V = new PdfName("V");
            V2 = new PdfName("V2");
            VALIGN = new PdfName("VAlign");
            VE = new PdfName("VE");
            VERISIGN_PPKVS = new PdfName("VeriSign.PPKVS");
            VERSION = new PdfName("Version");
            VERTICES = new PdfName("Vertices");
            VIDEO = new PdfName("Video");
            VIEW = new PdfName("View");
            VIEWS = new PdfName("Views");
            VIEWAREA = new PdfName("ViewArea");
            VIEWCLIP = new PdfName("ViewClip");
            VIEWERPREFERENCES = new PdfName("ViewerPreferences");
            VIEWPORT = new PdfName("Viewport");
            VIEWSTATE = new PdfName("ViewState");
            VISIBLEPAGES = new PdfName("VisiblePages");
            VOFFSET = new PdfName("VOffset");
            VP = new PdfName("VP");
            VRI = new PdfName("VRI");
            W = new PdfName("W");
            W2 = new PdfName("W2");
            WARICHU = new PdfName("Warichu");
            WATERMARK = new PdfName("Watermark");
            WC = new PdfName("WC");
            WIDGET = new PdfName("Widget");
            WIDTH = new PdfName("Width");
            WIDTHS = new PdfName("Widths");
            WIN = new PdfName("Win");
            WIN_ANSI_ENCODING = new PdfName("WinAnsiEncoding");
            WINDOW = new PdfName("Window");
            WINDOWED = new PdfName("Windowed");
            WIPE = new PdfName("Wipe");
            WHITEPOINT = new PdfName("WhitePoint");
            WKT = new PdfName("WKT");
            WP = new PdfName("WP");
            WS = new PdfName("WS");
            WT = new PdfName("WT");
            X = new PdfName("X");
            XA = new PdfName("XA");
            XD = new PdfName("XD");
            XFA = new PdfName("XFA");
            XML = new PdfName("XML");
            XOBJECT = new PdfName("XObject");
            XPTS = new PdfName("XPTS");
            XREF = new PdfName("XRef");
            XREFSTM = new PdfName("XRefStm");
            XSTEP = new PdfName("XStep");
            XYZ = new PdfName("XYZ");
            YSTEP = new PdfName("YStep");
            ZADB = new PdfName("ZaDb");
            ZAPFDINGBATS = new PdfName("ZapfDingbats");
            ZOOM = new PdfName("Zoom");
            FieldInfo[] fields = typeof(PdfName).GetFields(BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Public);
            staticNames = new Dictionary<string, PdfName>(fields.Length);
            try
            {
                foreach (FieldInfo fieldInfo in fields)
                {
                    if (fieldInfo.FieldType.Equals(typeof(PdfName)))
                    {
                        PdfName pdfName = (PdfName)fieldInfo.GetValue(null);
                        staticNames[DecodeName(pdfName.ToString())] = pdfName;
                    }
                }
            }
            catch
            {
            }
        }

        public PdfName(string name)
            : this(name, lengthCheck: true)
        {
        }

        public PdfName(string name, bool lengthCheck)
            : base(4)
        {
            int length = name.Length;
            if (lengthCheck && length > 127)
            {
                throw new ArgumentException(MessageLocalization.GetComposedMessage("the.name.1.is.too.long.2.characters", name, length));
            }

            bytes = EncodeName(name);
        }

        public PdfName(byte[] bytes)
            : base(4, bytes)
        {
        }

        public virtual int CompareTo(PdfName name)
        {
            byte[] array = bytes;
            byte[] array2 = name.bytes;
            int num = Math.Min(array.Length, array2.Length);
            for (int i = 0; i < num; i++)
            {
                if (array[i] > array2[i])
                {
                    return 1;
                }

                if (array[i] < array2[i])
                {
                    return -1;
                }
            }

            if (array.Length < array2.Length)
            {
                return -1;
            }

            if (array.Length > array2.Length)
            {
                return 1;
            }

            return 0;
        }

        public override bool Equals(object obj)
        {
            if (this == obj)
            {
                return true;
            }

            if (obj is PdfName)
            {
                return CompareTo((PdfName)obj) == 0;
            }

            return false;
        }

        public override int GetHashCode()
        {
            int num = hash;
            if (num == 0)
            {
                int num2 = 0;
                int num3 = bytes.Length;
                for (int i = 0; i < num3; i++)
                {
                    num = 31 * num + (bytes[num2++] & 0xFF);
                }

                hash = num;
            }

            return num;
        }

        public static byte[] EncodeName(string name)
        {
            ByteBuffer byteBuffer = new ByteBuffer(name.Length + 20);
            byteBuffer.Append('/');
            char[] array = name.ToCharArray();
            for (int i = 0; i < array.Length; i++)
            {
                char c = (char)(array[i] & 0xFFu);
                switch (c)
                {
                    case ' ':
                    case '#':
                    case '%':
                    case '(':
                    case ')':
                    case '/':
                    case '<':
                    case '>':
                    case '[':
                    case ']':
                    case '{':
                    case '}':
                        byteBuffer.Append('#');
                        byteBuffer.Append(Convert.ToString(c, 16));
                        continue;
                }

                if (c > '~' || c < ' ')
                {
                    byteBuffer.Append('#');
                    if (c < '\u0010')
                    {
                        byteBuffer.Append('0');
                    }

                    byteBuffer.Append(Convert.ToString(c, 16));
                }
                else
                {
                    byteBuffer.Append(c);
                }
            }

            return byteBuffer.ToByteArray();
        }

        public static string DecodeName(string name)
        {
            StringBuilder stringBuilder = new StringBuilder();
            int length = name.Length;
            for (int i = 1; i < length; i++)
            {
                char c = name[i];
                if (c == '#')
                {
                    c = (char)((PRTokeniser.GetHex(name[i + 1]) << 4) + PRTokeniser.GetHex(name[i + 2]));
                    i += 2;
                }

                stringBuilder.Append(c);
            }

            return stringBuilder.ToString();
        }
    }
}
