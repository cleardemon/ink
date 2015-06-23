﻿
namespace Inklewriter.Parsed
{
    public class DivertTarget : Expression
    {
        public Divert divert;

        public DivertTarget (Divert divert)
        {
            this.divert = AddContent(divert);
        }

        public override void GenerateIntoContainer (Runtime.Container container)
        {
            divert.GenerateRuntimeObject();

            _runtimeDivert = (Runtime.Divert) divert.runtimeDivert;
            _runtimeLiteralDivertTarget = new Runtime.LiteralDivertTarget (_runtimeDivert);
            _runtimeLiteralDivertTarget.divert = _runtimeDivert;

            if (divert.arguments != null && divert.arguments.Count > 0) {
                Error ("Can't use a divert target as a variable if it has parameters");
                return;
            }

            container.AddContent (_runtimeLiteralDivertTarget);
        }

        public override void ResolveReferences (Story context)
        {
            base.ResolveReferences (context);

            Parsed.Object usageContext = this;
            while (usageContext != null && usageContext is Expression) {

                bool badUsage = false;
                bool foundUsage = false;

                var usageParent = usageContext.parent;
                if (usageParent is BinaryExpression) {

                    // Only allowed to compare for equality

                    var binaryExprParent = usageParent as BinaryExpression;
                    if (binaryExprParent.opName != "==") {
                        badUsage = true;
                    }
                    foundUsage = true;
                }
                else if (usageParent is MultipleConditionExpression) {
                    badUsage = true;
                    foundUsage = true;
                } else if (usageParent is Choice && ((Choice)usageParent).condition == usageContext) {
                    badUsage = true;
                    foundUsage = true;
                } else if (usageParent is Conditional || usageParent is ConditionalSingleBranch) {
                    badUsage = true;
                    foundUsage = true;
                }

                if (badUsage) {
                    Error ("Can't use a divert target like that. Did you intend to call '" + divert.target + "' as a function: likeThis(), or check the read count: likeThis, with no arrows?", this);
                }

                if (foundUsage)
                    break;

                usageContext = usageParent;
            }

            _runtimeLiteralDivertTarget.divert = _runtimeDivert;
        }
            
        Runtime.LiteralDivertTarget _runtimeLiteralDivertTarget;
        Runtime.Divert _runtimeDivert;
    }
}

