 data1 Segment
val1     dw 056ah
omega    dd -312
alpha    db 'Prodan A.'
beta    equ 5+10*25-4*11
gamma    db 01001101b
ex       dw -68h
data1 ends

Code1 Segment  
      assume cs : Code1, ds : data1
;    mov eax, 26h
    cli
    inc al 
    mov dh, 123
@label:
    xor gamma[ebx+esi], 0110b
    xor omega[ebx+esi], -1d1ah
    jb @10
    dec alpha[ebx+esp]
    dec omega[ecx+ebx] ;rjvvtynfhbq
    cmp eax, edx
    jb @10
@10:
    cmp al, bh
    jb ;@label
    adc eax, omega[ebx+edi]
    adc bh, gs:alpha[edi+esi]
    div edx
    div dl
@20:
    mov edi, 3
    mov ah, -38
    jb @20
    and alpha[eax+edi], dl
    and omega[ebx+esi], edx
        Code1 ends
end





