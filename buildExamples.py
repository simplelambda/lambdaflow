import os
import shutil
import sys

def sync_examples():
    repo_root = os.path.abspath(os.path.dirname(__file__))
    framework_dir = os.path.join(repo_root, 'development', 'src')
    examples_root = os.path.join(repo_root, 'examples')

    if not os.path.isdir(framework_dir):
        print(f"Framework directory not found: {framework_dir}")
        sys.exit(1)
    if not os.path.isdir(examples_root):
        print(f"Examples directory not found: {examples_root}")
        sys.exit(1)

    examples = [
        name for name in os.listdir(examples_root)
        if os.path.isdir(os.path.join(examples_root, name))
    ]

    for example in examples:
        example_dir = os.path.join(examples_root, example)
        target_lf = os.path.join(example_dir, 'lambdaflow')

        print(f"Syncing example '{example}'")

        if os.path.isdir(target_lf):
            shutil.rmtree(target_lf)
        
        shutil.copytree(framework_dir, target_lf)

        build_py_src = os.path.join(repo_root, 'development', 'build.py')
        build_py_dst = os.path.join(example_dir, 'build.py')
        if os.path.isfile(build_py_src):
            shutil.copy2(build_py_src, build_py_dst)

        csproj_src = os.path.join(repo_root, 'development', 'lambdaflow.csproj')
        csproj_dst = os.path.join(example_dir, 'lambdaflow.csproj')
        if os.path.isfile(csproj_src):
            shutil.copy2(csproj_src, csproj_dst)
        
        print(f"Synchronized '{example}'")

    print("All examples synchronized.")

if __name__ == "__main__":
    sync_examples()