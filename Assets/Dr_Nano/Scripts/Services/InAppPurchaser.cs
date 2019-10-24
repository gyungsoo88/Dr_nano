using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;

public class InAppPurchaser : MonoBehaviour, IStoreListener
{
    private static IStoreController storeController;
    private static IExtensionProvider extensionProvider;

    //상품ID
    //상품ID는 구글 개발자 콘솔에 등록한 상품ID와 동일하게 해주세요.
    public const string productId1 = "coin1";
    public const string productId2 = "coin2";
    public const string productId3 = "coin3";
    public const string productId4 = "coin4";
    public const string productId5 = "coin5";
    public const string productId6 = "freepassAdView";

    // Start is called before the first frame update
    void Start()
    {
        InitializePurchasing();
    }

    private bool IsInitialized()
    {
        return (storeController != null && extensionProvider != null);
    }

    //인앱 매니저
    public void InitializePurchasing()
    {
        if (IsInitialized())
            return;

        var module = StandardPurchasingModule.Instance();

        ConfigurationBuilder builder = ConfigurationBuilder.Instance(module);

        //상품의 { 상품ID, 종류, 앱스토어에서 사용되는 이름 }
        builder.AddProduct(productId1, ProductType.Consumable, new IDs
        {
            { productId1, AppleAppStore.Name },
            { productId1, GooglePlay.Name },
        });

        builder.AddProduct(productId2, ProductType.Consumable, new IDs
        {
            { productId2, AppleAppStore.Name },
            { productId2, GooglePlay.Name },
        });

        builder.AddProduct(productId3, ProductType.Consumable, new IDs
        {
            { productId3, AppleAppStore.Name },
            { productId3, GooglePlay.Name },
        });

        builder.AddProduct(productId4, ProductType.Consumable, new IDs
        {
            { productId4, AppleAppStore.Name },
            { productId4, GooglePlay.Name },
        });

        builder.AddProduct(productId5, ProductType.Consumable, new IDs
        {
            { productId5, AppleAppStore.Name },
            { productId5, GooglePlay.Name },
        });

        builder.AddProduct(productId6, ProductType.Consumable, new IDs
        {
            { productId6, AppleAppStore.Name },
            { productId6, GooglePlay.Name },
        });

        UnityPurchasing.Initialize(this, builder);
    }

    //결제시도
    public void BuyProductID(string productId)
    {
        try
        {
            if (IsInitialized())
            {
                //가져온 상품정보 적용
                Product p = storeController.products.WithID(productId);

                if (p != null && p.availableToPurchase)
                {
                    //"비동기 상품 결제"
                    Debug.Log(string.Format("Purchasing product asychronously: '{0}'", p.definition.id));
                    storeController.InitiatePurchase(p); //결제시도
                }
                else
                {
                    //"구매할 상품의 ID: 상품결제 실패, 존재하지 않는 상품입니다."
                    Debug.Log("BuyProductID: FAIL, Not purchasing product, either is not found or is not available for purchase");
                }
            }
            else
            {
                //"구매할 상품의 ID를 찾을 수 없습니다. 초기화되지 않았습니다."
                Debug.Log("BuyProductID FAIL. Not initialized.");
            }
        }
        catch (Exception e)
        {
            //"try에서 발생한 예외 명시"
            Debug.Log("BuyProductID FAIL. Exception during purchase. " + e);
        }
    }

    //결제 복원관련
    public void RestorPurchase()
    {
        if (!IsInitialized())
        {
            //"결제 복원 실패. 초기화 안됨(중도취소 및 데이터 끊김)"
            Debug.Log("RestorePurchases FAIL. Not initialized.");
            return;
        }

        //아이폰 혹은 맥OS
        if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.OSXPlayer)
        {
            Debug.Log("RestorePurchase started ...");

            var apple = extensionProvider.GetExtension<IAppleExtensions>();

            //결제복원 result로 표시. result 메시지를 추가적으로 요구하지 않으면 복원할 결제목록이 없다
            apple.RestoreTransactions
                (
                    (result) => { Debug.Log("RestorePurchases continuing: " + result + ". If no further messages, no purchases available to restore."); }
                );

        }
        else
        {
            //"결제복원 실패. 해당 플랫폼에 대해 미지원"
            Debug.Log("RestorePurchases FAIL. Not supported on this platform. Current = " + Application.platform);
        }
    }

    //스토어 컨트롤러와 외부 확장자 초기화
    public void OnInitialized(IStoreController sc, IExtensionProvider ep)
    {
        Debug.Log("OnInitialized : PASS");

        storeController = sc;
        extensionProvider = ep;
    }

    //초기화 실패(이유)
    public void OnInitializeFailed(InitializationFailureReason reason)
    {
        Debug.Log("OnInitializeFailed InitializationFailureReason:" + reason);
    }

    //결제 성공
    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        //Debug.Log(string.Format("ProcessPurchase: PASS. Product: '{0}'", args.purchasedProduct.defnition.id));
        //Debug.Log(string.Format("결제 완료 , 구매 영수증 : '{0}'", args.purchasedProduct.defnition.id));

        switch (args.purchasedProduct.definition.id)
        {
            case productId1:
                //ex) coin 1000개 지급
                DataManager.Instance.AddCoins(1000);
                break;

            case productId2:
                //5000개 지급
                DataManager.Instance.AddCoins(5000);
                break;

            case productId3:
                //10000개 지급
                DataManager.Instance.AddCoins(10000);
                break;

            case productId4:
                //30000개 지급
                DataManager.Instance.AddCoins(30000);
                break;

            case productId5:
                //50000개 지급
                DataManager.Instance.AddCoins(50000);
                break;

            case productId6:
                //광고제거 적용
                DataManager.Instance.freepassAdView = true;
                break;

        }
        return PurchaseProcessingResult.Complete;
    }


    //결제 실패(상품 , 실패이유)
    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        Debug.Log(string.Format("OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}", product.definition.storeSpecificId, failureReason));
    }


}
