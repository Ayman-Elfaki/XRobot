
// ** Protocol Variables Start **

// Protocol Opreations

const uint8_t MOVE = 0xF0;

const uint8_t SNSE = 0x0F;

// Protocol Indicator

const uint8_t BGN = 0xAA;

const uint8_t FIN = 0x55;

const uint8_t VLD = BGN ^ FIN;

// ** Protocol Variables End **

uint8_t packets[5];

uint8_t DirMotorA = 12;
uint8_t SpdMotorA = 10;

uint8_t DirMotorB = 13;
uint8_t SpdMotorB = 11;

uint8_t LightSeensor = A5;

enum JoystickDirection
{
    Forward = 1,
    Backward = 2,
    Left = 4,
    Right = 8,
    None = 16,
};

void setup()
{
    Serial.begin(9600);

    pinMode(DirMotorA, OUTPUT);
    pinMode(DirMotorB, OUTPUT);

    pinMode(SpdMotorA, OUTPUT);
    pinMode(SpdMotorB, OUTPUT);
}

void loop()
{
    if (Serial.available())
    {
        Serial.readBytes(packets, 5);

        uint8_t command = packets[1];

        uint8_t packet = (packets[0] ^ packets[4]);

        if (packet == VLD)
        {
            if (command == MOVE)
            {
                uint8_t direction = packets[2];

                uint8_t speed = packets[3];

                if (direction == Forward)
                {
                    moveForward(speed, speed);
                }
                else if (direction == (Forward | Left))
                {
                    moveForward(speed, speed / 2.5f);
                }
                else if (direction == (Forward | Right))
                {
                    moveForward(speed / 2.5f, speed);
                }
                else if (direction == Backward)
                {
                    moveBackward(speed, speed);
                }
                else if (direction == (Backward | Left))
                {
                    moveBackward(speed, speed / 2.5f);
                }
                else if (direction == (Backward | Right))
                {
                    moveBackward(speed / 2.5f, speed);
                }
                else if (direction == Right)
                {
                    moveForward(speed * 0.0f, speed);
                }
                else if (direction == Left)
                {
                    moveForward(speed, speed * 0.0f);
                }
                else if (direction == None)
                {
                    stop();
                }
            }
            else if (command == SNSE)
            {
                uint8_t data = analogRead(LightSeensor);
                Serial.write(data);
            }
        }
    }
}

void moveForward(float speedA, float speedB)
{
    digitalWrite(DirMotorA, LOW);
    digitalWrite(DirMotorB, HIGH);

    analogWrite(SpdMotorA, speedA);
    analogWrite(SpdMotorB, speedB);
}

void moveBackward(float speedA, float speedB)
{
    digitalWrite(DirMotorA, HIGH);
    digitalWrite(DirMotorB, LOW);

    analogWrite(SpdMotorA, speedA);
    analogWrite(SpdMotorB, speedB);
}

void stop()
{
    digitalWrite(DirMotorA, LOW);
    digitalWrite(DirMotorB, LOW);

    analogWrite(SpdMotorA, 0);
    analogWrite(SpdMotorB, 0);
}