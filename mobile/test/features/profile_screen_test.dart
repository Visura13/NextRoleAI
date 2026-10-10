import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:nextroleai_mobile/features/cv/state/cv_selection_view_model.dart';
import 'package:nextroleai_mobile/features/profile/presentation/profile_screen.dart';
import 'package:nextroleai_mobile/features/profile/state/profile_view_model.dart';
import 'package:provider/provider.dart';

import '../support/fakes.dart';

void main() {
  testWidgets('shows CV evidence and preferences in one profile workspace', (
    tester,
  ) async {
    final cvRepository = FakeCvDataSource()
      ..profile = sampleCvProfile(status: 'Confirmed');

    await tester.pumpWidget(
      MultiProvider(
        providers: [
          ChangeNotifierProvider(
            create: (_) => ProfileViewModel(FakeProfileDataSource()),
          ),
          ChangeNotifierProvider(
            create: (_) =>
                CvSelectionViewModel(FakeCvFilePicker(), cvRepository),
          ),
        ],
        child: const MaterialApp(home: ProfileScreen()),
      ),
    );
    await tester.pumpAndSettle();

    expect(find.text('My profile'), findsOneWidget);
    expect(find.text('Your professional profile'), findsOneWidget);
    expect(find.text('alex-cv.pdf'), findsOneWidget);
    await tester.scrollUntilVisible(
      find.text('Job preferences'),
      500,
      scrollable: find.byType(Scrollable).first,
    );
    expect(find.text('Job preferences'), findsOneWidget);
    await tester.scrollUntilVisible(
      find.text('Preferred work location'),
      300,
      scrollable: find.byType(Scrollable).first,
    );
    expect(find.text('Preferred job title'), findsOneWidget);
    expect(find.text('Preferred work location'), findsOneWidget);
    expect(find.text('Professional headline'), findsNothing);
  });
}
