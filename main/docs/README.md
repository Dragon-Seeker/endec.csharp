## endec

endec is a format-agnostic serialization framework inspired by Rust's [serde](https://serde.rs) library and the Codec API from Mojang's [DataFixerUpper](https://github.com/mojang/datafixerupper).

This repository contains the C# implementation. The reference implementation (written in Dart) can be found [here](https://github.com/gliscowo/endec.dart) and the Java implementation can be found [here](https://github.com/wisp-forest/endec).

### Repository Structure

This project contains 3 submodules with the root src containing the core `endec` package which defines the API and contains some base implementations. The nested modules are as follows:

- `json`: Support for Json language using the [Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json) Library

### Documentation

For the time being, documentation can be found in the owo section of the [Wisp Forest docs](https://docs.wispforest.io/owo/endec). The linked document adequately explains the basics but is out-of-date - it will be updated in the future

### Acknowledgements

The excellent serde documentation and [enjarai's](https://enjarai.dev) Codec guide [on the Fabric Docs](https://docs.fabricmc.net/develop/codecs) have been invaluable during development. Further, [glisco](https://github.com/gliscowo) is responsible for developing the reference implementation
