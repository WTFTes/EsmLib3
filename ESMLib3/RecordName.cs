﻿namespace EsmLib3;

public enum RecordName : uint
{
    TES3 = 0x33534554,
    TES4 = 0x34534554,
    FORM = 0x4D524F46,
    HEDR = 0x52444548,
    MAST = 0x5453414D,
    DATA = 0x41544144,
    GMDT = 0x54444D47,
    SCRD = 0x44524353,
    SCRS = 0x53524353,
    GMST = 0x54534D47,
    FNAM = 0x4D414E46,
    STRV = 0x56525453,
    INTV = 0x56544E49,
    FLTV = 0x56544C46,
    STTV = 0x56545453,
    NAME = 0x454D414E,
    GLOB = 0x424F4C47,
    DELE = 0x454C4544,
    CLAS = 0x53414C43,
    CLDT = 0x54444C43,
    DESC = 0x43534544,
    FACT = 0x54434146,
    RNAM = 0x4D414E52,
    FADT = 0x54444146,
    ANAM = 0x4D414E41,
    RACE = 0x45434152,
    RADT = 0x54444152,
    NPCS = 0x5343504E,
    SOUN = 0x4E554F53,
    SKIL = 0x4C494B53,
    INDX = 0x58444E49,
    SKDT = 0x54444B53,
    MGEF = 0x4645474D,
    MEDT = 0x5444454D,
    ITEX = 0x58455449,
    PTEX = 0x58455450,
    BSND = 0x444E5342,
    CSND = 0x444E5343,
    HSND = 0x444E5348,
    ASND = 0x444E5341,
    CVFX = 0x58465643,
    BVFX = 0x58465642,
    HVFX = 0x58465648,
    AVFX = 0x58465641,
    SCPT = 0x54504353,
    SCHD = 0x44484353,
    SCVR = 0x52564353,
    SCDT = 0x54444353,
    SCTX = 0x58544353,
    REGN = 0x4E474552,
    WEAT = 0x54414557,
    BNAM = 0x4D414E42,
    CNAM = 0x4D414E43,
    SNAM = 0x4D414E53,
    BSGN = 0x4E475342,
    TNAM = 0x4D414E54,
    LTEX = 0x5845544C,
    STAT = 0x54415453,
    MODL = 0x4C444F4D,
    DOOR = 0x524F4F44,
    SCRI = 0x49524353,
    SPEL = 0x4C455053,
    ENAM = 0x4D414E45,
    SPDT = 0x54445053,
    MISC = 0x4353494D,
    MCDT = 0x5444434D,
    WEAP = 0x50414557,
    WPDT = 0x54445057,
    CONT = 0x544E4F43,
    CNDT = 0x54444E43,
    FLAG = 0x47414C46,
    NPCO = 0x4F43504E,
    CREA = 0x41455243,
    ATTR = 0x52545441,
    AI_W = 0x575F4941,
    AI_T = 0x545F4941,
    AI_F = 0x465F4941,
    AI_E = 0x455F4941,
    AI_A = 0x415F4941,
    DODT = 0x54444F44,
    DNAM = 0x4D414E44,
    NPDT = 0x5444504E,
    XSCL = 0x4C435358,
    AIDT = 0x54444941,
    BODY = 0x59444F42,
    BYDT = 0x54445942,
    LIGH = 0x4847494C,
    LHDT = 0x5444484C,
    ENCH = 0x48434E45,
    ENDT = 0x54444E45,
    NPC_ = 0x5F43504E,
    KNAM = 0x4D414E4B,
    ARMO = 0x4F4D5241,
    AODT = 0x54444F41,
    CLOT = 0x544F4C43,
    CTDT = 0x54445443,
    REPA = 0x41504552,
    RIDT = 0x54444952,
    ACTI = 0x49544341,
    APPA = 0x41505041,
    AADT = 0x54444141,
    LOCK = 0x4B434F4C,
    LKDT = 0x54444B4C,
    PROB = 0x424F5250,
    PBDT = 0x54444250, 
    INGR = 0x52474E49,
    IRDT = 0x54445249,
    BOOK = 0x4B4F4F42,
    BKDT = 0x54444B42,
    TEXT = 0x54584554,
    ALCH = 0x48434C41,
    ALDT = 0x54444C41,
    LEVI = 0x4956454C,
    NNAM = 0x4D414E4E,
    LEVC = 0x4356454C,
    INAM = 0x4D414E49,
    CELL = 0x4C4C4543,
    WHGT = 0x54474857,
    AMBI = 0x49424D41,
    RGNN = 0x4E4E4752,
    NAM5 = 0x354D414E,
    NAM0 = 0x304D414E,
    LAND = 0x444E414C,
    VNML = 0x4C4D4E56,
    VHGT = 0x54474856,
    WNAM = 0x4D414E57,
    VCLR = 0x524C4356,
    VTEX = 0x58455456,
    FRMR = 0x524D5246,
    MVRF = 0x4652564D,
    PGRD = 0x44524750,
    PGRP = 0x50524750,
    PGRC = 0x43524750,
    NAM9 = 0x394D414E,
    XSOL = 0x4C4F5358,
    XCHG = 0x47484358,
    UNAM = 0x4D414E55,
    SNDG = 0x47444E53,
    DIAL = 0x4C414944,
    ID__ = 0x5F5F4449,
    INFO = 0x4F464E49,
    PNAM = 0x4D414E50,
    ONAM = 0x4D414E4F,
    QSTN = 0x4E545351,
    QSTF = 0x46545351,
    QSTR = 0x52545351,
    SSCR = 0x52435353,
    SLCS = 0x53434C53,
    SLSD = 0x44534C53,
    SLLD = 0x444C4C53,
    SLFD = 0x44464C53,
}
