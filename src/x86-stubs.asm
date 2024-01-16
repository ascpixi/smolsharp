.586
.xmm
.model  flat

.const
align 8

_Int32ToUInt32  DQ  0000000000000000H
                DQ  41F0000000000000H       ; 2**32
_DP2to32        EQU (_Int32ToUInt32+8)

.code

PUBLIC RhpLng2Dbl
RhpLng2Dbl PROC
  mov      edx,dword ptr [esp+8]
  mov      ecx,dword ptr [esp+4]
  xorps    xmm1,xmm1
  cvtsi2sd xmm1,edx              ; convert (signed) upper bits
  xorps    xmm0,xmm0
  cvtsi2sd xmm0,ecx              ; convert (signed) lower bits
  shr      ecx,31                ; get sign bit into ecx
  mulsd    xmm1,_DP2to32         ; adjust upper value for bit position
  addsd    xmm0,_Int32ToUInt32[ecx*8] ; adjust to unsigned
  addsd    xmm0,xmm1             ; combine upper and lower
  movsd    mmword ptr [esp+4],xmm0
  fld      qword ptr [esp+4]
  ret
RhpLng2Dbl ENDP

PUBLIC RhpByRefAssignRef
RhpByRefAssignRef PROC
  movs    dword ptr es:[edi],dword ptr [esi]  
  ret
RhpByRefAssignRef ENDP

PUBLIC RhpCheckedAssignRefEAX
RhpCheckedAssignRefEAX PROC
  ;
  ; This is not tested. Once you validate this does what it should, remove this int 3
  int 3
  ;

  mov     DWORD PTR [edx], eax
  ret
RhpCheckedAssignRefEAX ENDP


end
