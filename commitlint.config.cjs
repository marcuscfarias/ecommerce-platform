module.exports = {
  extends: ['@commitlint/config-conventional'],
  // Dependabot capitalizes "Bump" on nuget updates and the casing is not configurable.
  ignores: [(message) => /^build\(deps(-dev)?\): Bump /.test(message)],
};
