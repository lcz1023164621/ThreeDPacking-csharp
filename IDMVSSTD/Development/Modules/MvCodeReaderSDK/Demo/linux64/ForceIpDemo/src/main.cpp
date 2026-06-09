#include "ForceIpDemo.h"
#include <QApplication>

int main(int argc, char *argv[])
{
    QApplication a(argc, argv);
    CForceIpDemo w;
    w.show();

    return a.exec();
}
