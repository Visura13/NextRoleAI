import 'package:file_picker/file_picker.dart';

class SelectedCv {
  const SelectedCv({required this.name, required this.sizeInBytes});

  final String name;
  final int sizeInBytes;
}

abstract interface class CvFilePicker {
  Future<SelectedCv?> pick();
}

class DeviceCvFilePicker implements CvFilePicker {
  @override
  Future<SelectedCv?> pick() async {
    final file = await FilePicker.pickFile(
      type: FileType.custom,
      allowedExtensions: const ['pdf', 'doc', 'docx'],
    );
    if (file == null) return null;
    final length = file.lengthSync() ?? await file.length() ?? 0;
    return SelectedCv(name: file.name, sizeInBytes: length);
  }
}
