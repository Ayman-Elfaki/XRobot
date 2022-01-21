; Created: 1/13/2018 
; Author : Ayman Elfaki

.device "ATmega328P"

.equ freq = 16000000
.equ r_buad = 9600
.equ buad = (freq/(16*r_buad)-1)

.def temp1 = r16
.def temp2 = r17
.def counter = r18
.def rx_data = r19
.def tx_data = r20

.equ cForward = 1
.equ cBackward = 2
.equ cLeft = 4
.equ cRight = 8
.equ cNone = 16

.equ cBGN = 0xAA
.equ cEND = 0x55

.equ cValid = 0xFF

.equ cMOVE = 0xF0
.equ cSense = 0x0F


.macro transmitPacketMacro
	
wait:
	lds	temp1, UCSR0A			; Load the USART Control Register Data
	sbrs temp1, UDRE0			; wait for empty transmit buffer
	rjmp wait

	sts UDR0, @0

.endmacro

.macro recievePacketMacro

wait:	

	lds	temp1, UCSR0A			; Load the USART Control Register Data
	sbrs temp1, RXC0			; wait for empty recieve buffer
	rjmp wait
	
	lds @0, UDR0				; Load the recieved data into the register	

.endmacro

.macro flushMacro

flush:	

	lds	temp1, UCSR0A			; Load the USART Control Register Data
	sbrs temp1, RXC0			; wait for empty recieve buffer
	rjmp done 
	
	lds temp1, UDR0				; Load the recieved data into the register	
	rjmp flush

done:

.endmacro


.macro conditionMacro 

	lds temp1, packets+2
	cpi temp1, @0
	breq true

	jmp false

true:

	ldi temp1, @0
	cpi temp1, (cForward)
	breq forward

	ldi temp1, @0
	cpi temp1, (cForward | cLeft)
	breq forward

	ldi temp1, @0
	cpi temp1, (cForward | cRight)
	breq forward

	ldi temp1, @0
	cpi temp1, (cLeft)
	breq forward

	ldi temp1, @0
	cpi temp1, (cRight)
	breq forward

	jmp backward

forward:

	ldi temp1, (0 << PORTB4) | (1 << PORTB5)
	out PORTB, temp1
	jmp set_speed

backward:

	ldi temp1, (1 << PORTB4) | (0 << PORTB5)
	out PORTB, temp1
	jmp set_speed

set_speed:

	pop temp1
    sts OCR2A, temp1

	pop temp1
    sts OCR1BL, temp1

	rjmp loop

false:

	pop temp1
	pop temp1

.endmacro

.macro moveRobotMacro
	
	lds temp1, packets+3
	push temp1
	push temp1

	conditionMacro cForward

	lds temp1, packets+3
	push temp1
	push temp1

	conditionMacro cBackward

	lds temp1, packets+3
	push temp1

	ldi temp1, 0
	push temp1

	conditionMacro cLeft

	ldi temp1, 0
	push temp1

	lds temp1, packets+3
	push temp1

	conditionMacro cRight

	lds temp1, packets+3
	push temp1

	lsr temp1
	push temp1

	conditionMacro (cForward | cLeft)

	lsr temp1
	push temp1

	lds temp1, packets+3
	push temp1
	
	conditionMacro (cForward | cRight)
	
	lds temp1, packets+3
	push temp1
	
	lsr temp1
	push temp1

	conditionMacro (cBackward | cLeft)

	lsr temp1
	push temp1
	
	lds temp1, packets+3
	push temp1
	
	conditionMacro (cBackward | cRight)

	ldi temp1, 0
	push temp1

	push temp1
	
	conditionMacro cNone

.endmacro

.macro transmitNBytesMacro

	clr counter
		
	ldi	XL, LOW(packets)		; initialize X pointer
	ldi	XH, HIGH(packets)		; to packets address
		
transmit_packets:

	ld tx_data, X+
	transmitPacketMacro tx_data

	inc counter
	cpi counter, @0
	brne transmit_packets

.endmacro


.macro recieveNBytesMacro

	clr counter
		
	ldi	XL, LOW(packets)		; initialize X pointer
	ldi	XH, HIGH(packets)		; to packets address
	
recieve_packets:
	
	recievePacketMacro rx_data
	st X+, rx_data

	inc counter
	cpi counter, @0
	brne recieve_packets

.endmacro



.macro readSensorMacro
	; START ADC Converter
	ldi temp1, (1 << ADSC)|(1 << ADEN)|(1 << ADPS0)|(1 << ADPS1)|(1 << ADPS2)  
	sts ADCSRA, temp1	
	; Delay until conversion is finshed
adc_delay:
	ldi temp1, ADCSRA			
	sbrs temp1, ADIF
	rjmp adc_delay

	lds @0, ADCL			; The Value From the ADC, low byte
	lds @1, ADCH			; The Value From the ADC, high byte
	
	; STOP ADC Converter
	ldi temp1, (0 << ADSC)|(1 << ADEN)|(1 << ADPS0)|(1 << ADPS1)|(1 << ADPS2)	
	sts ADCSRA,temp1

.endmacro


.macro recieveCorrectionMacro

recieve_packets:
	
	recievePacketMacro rx_data
	
	cpi rx_data, cEND
	brne recieve_packets

.endmacro


.dseg
.org	SRAM_START
	packets:	.byte	5
.cseg
.org 0x00


setup:
	;*** I/O Ports Setup ***;
		
	ldi temp1, (1 << PORTB2) | (1 << PORTB3) | (1 << PORTB4) | (1 << PORTB4)
	out DDRB,temp1
	
	;*** ADC Setup ***;
	; Get the ADC05 pin reading
	ldi temp1, (1 << REFS0) | (1 << MUX0) | (1 << MUX2) 
	sts ADMUX,temp1
	; Enable the ADC in Single Conversion Mode and with prescaler 128
	ldi temp1, (1 << ADEN)|(1 << ADPS0)|(1 << ADPS1)|(1 << ADPS2)
	sts ADCSRA, temp1

	;*** USART Setup ***;
	; Set baud rate to UBRR0
	ldi temp1,(buad >> 8)
	sts UBRR0H, temp1
	ldi temp1, (buad)
	sts UBRR0L, temp1
	; Enable receiver and transmiter
	ldi temp1, (1 << RXEN0) |(1 << TXEN0)
	sts UCSR0B,temp1
	; Set frame format: 8 bit data
	ldi temp1, (1 << UCSZ01) | (1 << UCSZ00)
	sts UCSR0C,temp1

	;*** PWM Setup ***;
	; First Counter For PIN 10
	
	; PWM mode = Fast PWM 8-bit Set No Prescaler
	ldi temp1, (1 << COM1B1) | (1 << WGM10)
	sts TCCR1A, temp1
	ldi temp1,  (1 << WGM12) | (1 << CS10)
	sts TCCR1B, temp1


    ; Second Counter For PIN 11 Set No PRESCALER
	ldi temp1, (1 << COM2A1) | (1 << WGM21)| (1 << WGM20)
	sts TCCR2A, temp1
	ldi temp1, (1 << CS20)
	sts TCCR2B, temp1


loop:

	; Fill the packets buffer
	recieveNBytesMacro 5

	;*** Validation ***


	; Load BGN Packet
	lds temp1, packets
	; Load END Packet
	lds temp2, packets+4

	; Validate Packet
	eor temp1, temp2
	cpi temp1, cValid
    breq valid_packet
	
 	rjmp loop

 valid_packet:

	lds temp1, packets+1
	cpi temp1, cMOVE
	brne cond_1

	rjmp move_robot

cond_1:

	lds temp1, packets+1
	cpi temp1, cSense
	brne cond_2

	rjmp read_sensor

cond_2:

	rjmp loop

read_sensor:

	ldi tem1, cBGN
	transmitPacketMacro temp1
	
	ldi tem1, cSense
	transmitPacketMacro temp1


	readSensorMacro temp1, temp2
	
	transmitPacketMacro temp1
	
	transmitPacketMacro temp2

	
	ldi tem1, cEnd
	transmitPacketMacro temp1

	rjmp loop


move_robot:

	moveRobotMacro

	rjmp loop
