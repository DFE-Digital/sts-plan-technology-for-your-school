echo "Adding this line in the hope it kicks off the pipelines"
cd ..
echo
echo "Changing directory to /contentful/"
cd contentful
echo "Reinstalling node modules and fixing vulnerabilities"
npx reinstall
npm audit fix
echo
echo "================================"
echo "Changing directory to /contentful/content-management/"
cd content-management
echo "Reinstalling node modules and fixing vulnerabilities"
npx reinstall
npm audit fix
echo
echo "================================"
echo "Changing directory to /tests/Dfe.PlanTech.Web.Node.UnitTests/"
cd ../../tests/Dfe.PlanTech.Web.Node.UnitTests/
echo "Reinstalling node modules and fixing vulnerabilities"
npx reinstall
npm audit fix
echo
echo "================================"
echo "Changing directory to /tests/Dfe.PlanTech.Web.E2ETests.Beta/"
cd ../Dfe.PlanTech.Web.E2ETests.Beta/
echo "Reinstalling node modules and fixing vulnerabilities"
npx reinstall
npm audit fix
echo
echo "================================"
echo "Changing directory to root"
cd ../..
echo "Reinstalling node modules and fixing vulnerabilities"
npx reinstall
npm audit fix
