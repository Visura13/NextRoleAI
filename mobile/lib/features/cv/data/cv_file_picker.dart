import 'package:file_picker/file_picker.dart';
import 'package:flutter/foundation.dart';

class SelectedCv {
  const SelectedCv({
    required this.name,
    required this.sizeInBytes,
    required this.bytes,
  });

  final String name;
  final int sizeInBytes;
  final Uint8List bytes;
}

abstract interface class CvFilePicker {
  Future<SelectedCv?> pick();
}

class DeviceCvFilePicker implements CvFilePicker {
  @override
  Future<SelectedCv?> pick() async {
    final file = await FilePicker.pickFile(
      type: FileType.custom,
      allowedExtensions: const ['pdf', 'docx'],
    );
    if (file == null) return null;
    final length = file.lengthSync() ?? await file.length() ?? 0;
    return SelectedCv(
      name: file.name,
      sizeInBytes: length,
      bytes: await file.readAsBytes(),
    );
  }
}
