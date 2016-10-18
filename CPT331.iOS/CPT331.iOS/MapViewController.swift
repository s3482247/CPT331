//
//  MapViewController.swift
//  SpatioNews
//
//  Created by Peter Weller on 15/09/2016.
//  Copyright © 2016 Peter Weller. All rights reserved.
//

import UIKit
import Mapbox

class MapViewController: UIViewController, MGLMapViewDelegate, MapViewModelDelegate, UIGestureRecognizerDelegate, LocationSearchDelegate {
    
    // -----------------------------
    // MARK: Constants
    // -----------------------------
    let selectionZoomLevel:Double = 12.5
    let annotationImage = UIImage(named: "Event-Annotation.png")
    let centerOffsets:CoordinateOffset = (
        top: 85, // Search bar
        right: 0,
        bottom: 400, // Subview
        left: 0
    )
    
    
    
    // -----------------------------
    // MARK: Runtime Variables
    // -----------------------------
    var viewModel:MapViewModel = MapViewModel()
    
    // Stores the most recently tapped location label
    // Used to pass the location to child views
    var lastLocationTapped:Location?
    var lastEventTapped:Event?
    
    // Used to prevent map updatesw while panning
    var mapRegionIsChanging:Bool=false
    
    
    
    // -----------------------------
    // MARK: Storyboard References
    // -----------------------------
    @IBOutlet weak var mapView: MGLMapView!
    @IBOutlet weak var locationSearchView: LocationSearchView!
    
    
    
    // -----------------------------
    // MARK: Main Logic
    // -----------------------------
    override func viewDidLoad() {
        super.viewDidLoad()

        // Setup map
        self.mapView.delegate = self
        self.viewModel.delegate = self
        
        // Setup search
        self.locationSearchView.delegate = self
        
        // Register double tap recognizer so the single tap knows to ignore them
        let doubleTap = UITapGestureRecognizer(target: self, action: nil)
        doubleTap.numberOfTapsRequired = 2
        self.mapView.addGestureRecognizer(doubleTap)

        // Register single tap recognizer to check if the user tapped on a city/town/village label
        let singleTap = UITapGestureRecognizer(target: self, action: #selector(self.gestureRecognizerBegan))
        singleTap.requireGestureRecognizerToFail(doubleTap)
        singleTap.delegate = self
        singleTap.cancelsTouchesInView = false
        self.mapView.addGestureRecognizer(singleTap)
    }
    
    override func didReceiveMemoryWarning() {
        super.didReceiveMemoryWarning()
        // Dispose of any resources that can be recreated.
    }
    
    func menuButtonTapped(button: UIButton) {
        print("Menu button tapped!")
    }
    
    func getUserLocation() -> CLLocation? {
        return self.mapView.userLocation?.location
    }
    
    func selectLocation(location:Location, pan:Bool=true, zoom:Bool=true) {
        if pan && zoom {
            self.mapView.setCenterCoordinate(location.coordinate, zoomLevel: self.selectionZoomLevel, animated: true, withOffset: self.centerOffsets)
        } else if pan {
            self.mapView.setCenterCoordinate(location.coordinate, zoomLevel: nil, animated: true, withOffset: self.centerOffsets)
        }
        
        self.lastLocationTapped = location
        self.performSegueWithIdentifier("showLocationView", sender: nil)
    }
    
    func selectEvent(event:Event, pan:Bool=true, zoom:Bool=true) {
        guard let coordinate = event.coordinate else {
            return
        }
        
        if pan && zoom {
            self.mapView.setCenterCoordinate(coordinate, zoomLevel: self.selectionZoomLevel, animated: true, withOffset: self.centerOffsets)
        } else if pan {
            self.mapView.setCenterCoordinate(coordinate, zoomLevel: nil, animated: true, withOffset: self.centerOffsets)
        }
        
        self.mapView.setCenterCoordinate(coordinate, animated: true, withOffset: self.centerOffsets)
        
        
        self.lastEventTapped = event
        self.performSegueWithIdentifier("showEventView", sender: nil)
    }
    
    override func prepareForSegue(segue: UIStoryboardSegue, sender: AnyObject?) {
        if segue.identifier == "showLocationView", let location = self.lastLocationTapped {
            let vc = segue.destinationViewController as! ModalViewController
            vc.location = location
            
        } else if segue.identifier == "showEventView", let event = self.lastEventTapped {
            let vc = segue.destinationViewController as! ModalViewController
            vc.event = event
        }
    }
    
    
    
    // -----------------------------
    // MARK: Map Drawing
    // -----------------------------
    func showAnnotations(forEvents events:[Int:Event]) {
        // Only update map if not already panning
        guard self.mapRegionIsChanging == false else {
            return
        }
        
        // Remove existing annotations
        if let existing = mapView.annotations {
            self.mapView.removeAnnotations(existing)
        }
        
        // Filter events without coordinates
        let filteredEvents = events.filter{$0.1.coordinate != nil}
        
        // Build annotations array
        let annotations = filteredEvents.map{(_,event) in EventPointFeature(event: event)}
        
        // Add annotations
        self.mapView.addAnnotations(annotations)
    }

    // Returns a reusable annotation image which reflects the event category
    func mapView(mapView: MGLMapView, imageForAnnotation annotation: MGLAnnotation) -> MGLAnnotationImage? {
        // Only event features with event categories should have custom images
        guard let category = (annotation as? EventPointFeature)?.event.category else {
            return nil
        }
        
        // Get the resuse identifier for the annotation
        let reuseIdentifier = category.name
        
        // Try to reuse an existing annotation image, if it exists
        var reusableImage = mapView.dequeueReusableAnnotationImageWithIdentifier(reuseIdentifier)
        
        // if the annotation image hasn‘t been used yet, initialize it here with the reuse identifier
        if reusableImage == nil, var image = self.annotationImage?.tintWithColor(category.color) {
            
            // Set the image anchor to the bottom (By default it is center)
            image = image.imageWithAlignmentRectInsets(UIEdgeInsetsMake(0, 0, image.size.height/2, 0))
            reusableImage = MGLAnnotationImage(image: image, reuseIdentifier: reuseIdentifier)
        }
        
        return reusableImage
    }
    
    
    
    // -----------------------------
    // MARK: Map event responders
    // -----------------------------
    func mapView(mapView: MGLMapView, regionWillChangeAnimated animated: Bool) {
        self.mapRegionIsChanging = true
        self.dismissKeyboard()
    }
    
    // When map region changes, load events for visible region
    func mapView(mapView: MGLMapView, regionDidChangeAnimated animated: Bool) {
        self.mapRegionIsChanging = false
        self.viewModel.loadEvents(forMapView: mapView)
    }
    
    // Annotation responder
    func mapView(mapView: MGLMapView, didSelectAnnotation annotation: MGLAnnotation) {
        if let event = (annotation as? EventPointFeature)?.event {
            self.selectEvent(event, zoom:false)
        }
    }
    
    // Checks to see if the tap gesture should be recognized. If there is a marker present at the tap
    //  location, the gesture will not be recognized. This is necessary, because (by default) a tap
    //  gesture will intercept all taps and prevent the maps own recognizers from working.
    func gestureRecognizerShouldBegin(gestureRecognizer: UIGestureRecognizer) -> Bool {
        
        // Dismiss keyboard whenever the map is tapped
        self.dismissKeyboard()
        
        if let tap = gestureRecognizer as? UITapGestureRecognizer {
            
            // Iterate over each feature, attempting to find an annotation
            for feature in self.mapView.visiblePointFeatures(atGestureLocation: tap) {
                
                // If it doesn't have a name attribute, it's probably an annotation?
                if feature.attributeForKey("name") == nil {
                    return false
                }
            }
        }
        
        return true
    }
    
    func gestureRecognizerBegan(gestureRecognizer: UIGestureRecognizer) {
        let pointFeatures = self.mapView.visiblePointFeatures(atGestureLocation: gestureRecognizer)
        
        // If it has a name attribute, assume it's a location label
        if pointFeatures.count > 0, let name = pointFeatures[0].attributeForKey("name") as? String {
            let feature = pointFeatures[0]
            
            if let type = feature.attributeForKey("type") as? String {
                // Convert object to make it easier to work with
                let location = Location(name: name, type: type, coordinate: feature.coordinate)
                self.selectLocation(location, zoom:false)
            }
        }
    }
}