function validateDatabase(db) {
  const errors = [];
  console.log('\n📋 Validating database...');

  const orphanedUserOrgs = db
    .prepare(
      `
    SELECT COUNT(*) as count FROM user_organisation_mappings
    WHERE user_id NOT IN (SELECT user_id FROM users)
      OR organisation_id NOT IN (SELECT organisation_id FROM organisations)
  `,
    )
    .get();
  if (orphanedUserOrgs.count > 0)
    errors.push(`❌ Found ${orphanedUserOrgs.count} orphaned user-organization mappings`);

  const orphanedRoles = db
    .prepare(
      `
    SELECT COUNT(*) as count FROM roles WHERE service_id NOT IN (SELECT service_id FROM services)
  `,
    )
    .get();
  if (orphanedRoles.count > 0) errors.push(`❌ Found ${orphanedRoles.count} orphaned roles`);

  const orphanedUserServiceRoles = db
    .prepare(
      `
    SELECT COUNT(*) as count FROM user_service_roles
    WHERE user_id NOT IN (SELECT user_id FROM users)
      OR service_id NOT IN (SELECT service_id FROM services)
      OR role_id NOT IN (SELECT role_id FROM roles)
  `,
    )
    .get();
  if (orphanedUserServiceRoles.count > 0)
    errors.push(`❌ Found ${orphanedUserServiceRoles.count} orphaned user-service-role mappings`);

  const duplicateUsers = db
    .prepare(
      `
    SELECT email, COUNT(*) as count FROM users GROUP BY email HAVING count > 1
  `,
    )
    .all();
  if (duplicateUsers.length > 0)
    errors.push(
      `❌ Found ${duplicateUsers.length} duplicate user emails: ${duplicateUsers.map((u) => u.email).join(', ')}`,
    );

  if (errors.length === 0) {
    const n = (sql) => db.prepare(sql).get().count;
    const userCount = n(`SELECT COUNT(*) as count FROM users WHERE user_source = 'DfE Sign In'`);
    const discoveredCount = n(
      `SELECT COUNT(*) as count FROM users WHERE user_source = 'Discovered'`,
    );

    console.log('✅ Validation passed!');
    console.log(
      `   Users: ${n('SELECT COUNT(*) as count FROM users')} (${userCount} DSI references, ${discoveredCount} discovered)`,
    );
    console.log(`   Organizations: ${n('SELECT COUNT(*) as count FROM organisations')}`);
    console.log(`   Services: ${n('SELECT COUNT(*) as count FROM services')}`);
    console.log(`   Roles: ${n('SELECT COUNT(*) as count FROM roles')}`);
    console.log(
      `   User-Org Mappings: ${n('SELECT COUNT(*) as count FROM user_organisation_mappings')}`,
    );
    console.log(
      `   User-Service-Role Mappings: ${n('SELECT COUNT(*) as count FROM user_service_roles')}`,
    );
  }

  return errors;
}

module.exports = { validateDatabase };
